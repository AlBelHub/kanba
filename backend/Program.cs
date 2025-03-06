using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapper;
using static BCrypt.Net.BCrypt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;

namespace backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var jwtIssuer = builder.Configuration["Jwt:Issuer"] 
                        ?? throw new InvalidOperationException("JWT Issuer is not configured.");
        var jwtAudience = builder.Configuration["Jwt:Audience"] 
                          ?? throw new InvalidOperationException("JWT Audience is not configured.");
        var jwtKey = builder.Configuration["Jwt:Key"] 
                     ?? throw new InvalidOperationException("JWT Key is not configured.");

        
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            // Добавляем поддержку заголовка Authorization для Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Description = "Введите токен в формате: Bearer {your_token}",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });
        
        builder.Services.AddSingleton<IDbConnection>(_ =>
        {
            var connectionString = builder.Configuration.GetConnectionString("Postgres")
                                   ?? throw new InvalidOperationException("Postgres connection string is missing.");
            return new NpgsqlConnection(connectionString);
        });
        
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

        builder.Services.AddAuthorization();
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapPost("/login", [AllowAnonymous] async ( [FromBody] UserModel login, IDbConnection db) =>
        {
            if (string.IsNullOrWhiteSpace(login.username) || string.IsNullOrWhiteSpace(login.password))
            {
                return Results.BadRequest("Invalid credentials");
            }
            
            using var cmd = db.CreateCommand();
        
            string CommandText = "SELECT password FROM Users WHERE username = @username";

            var args = new { username = login.username };
        
            var storedPasswordHash = await db.ExecuteScalarAsync(CommandText, args) as string;
            if (storedPasswordHash == null || !Verify(login.password, storedPasswordHash))
            {
                return Results.BadRequest("Invalid credentials.");
            }

            var tokenString = GenerateJSONWebToken(login);
            return Results.Ok(new { token = tokenString });
            
        });

        app.MapPost("/register", [AllowAnonymous] async ([FromBody] UserModel newUser, IDbConnection db) =>
        {
            if (string.IsNullOrWhiteSpace(newUser.username) || string.IsNullOrWhiteSpace(newUser.password))
            {
                return Results.BadRequest("Username and Password are required.");
            }

            // Проверяем, существует ли пользователь
            string userExistCommand = "SELECT COUNT(*) FROM Users WHERE Username = @username";
            var args = new { username = newUser.username };
    

            var userExists = db.ExecuteScalar<long>(userExistCommand, args) > 0;
            if (userExists)
            {
                return Results.BadRequest("User already exists.");
            }

            // Хешируем пароль с использованием bcrypt
            var passwordHash = HashPassword(newUser.password, GenerateSalt());

            // Сохраняем пользователя
    
            string CommandText = "INSERT INTO Users (username, password) VALUES (@username, @passwordHash)";
    
            var QueryArgs = new { username = newUser.username, passwordHash = passwordHash };
    
            await db.ExecuteAsync(CommandText, QueryArgs);

            return Results.Ok("User registered successfully.");
        });
        
        app.MapGet("/validate_token", [Authorize] () => Results.Ok("You have access to this secure endpoint"));
        
        app.MapGet("/users", async (IDbConnection db) =>
        {
            var users = await db.QueryAsync("SELECT id, username FROM Users");
            return Results.Ok(users);
        });

        app.MapGet("/spaces", async (IDbConnection db) =>
        {
            var spaces = await db.QueryAsync("SELECT id, name, user_id FROM Spaces");
            return Results.Ok(spaces);
        });

        app.MapGet("/boards", async (IDbConnection db) =>
        {
            var boards = await db.QueryAsync("SELECT id, name, space_id, owner_id FROM Boards");
            return Results.Ok(boards);
        });

        app.MapGet("/columns", async (IDbConnection db) =>
        {
            var columns = await db.QueryAsync("SELECT id, board_id, title, created_by FROM Columns");
            return Results.Ok(columns);
        });

        app.MapGet("/tasks", async (IDbConnection db) =>
        {
            var tasks = await db.QueryAsync("SELECT id, column_id, title, description, status, created_by, assigned_to FROM Tasks");
            
            return Results.Ok(tasks);
        });
        
        EnsureDatabaseCreated(app.Services.GetRequiredService<IDbConnection>());
        
        app.Run();
       
        string GenerateJSONWebToken(UserModel userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(jwtIssuer,
                jwtIssuer,
                claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
            
        }
        void EnsureDatabaseCreated(IDbConnection db)
        {
            // Создание таблицы пользователей
            string createUsers = @"
            CREATE TABLE IF NOT EXISTS Users (
               id serial primary key,
               username varchar(40) not null,
               password varchar(255) not null
            );";
            db.Execute(createUsers);

            // Таблица для пространств (spaces) — личное рабочее пространство пользователя
            string createSpaces = @"
            CREATE TABLE IF NOT EXISTS Spaces (
               id serial primary key,
               name varchar(50) not null,
               user_id int REFERENCES Users(id) ON DELETE CASCADE,
               created_at timestamp default current_timestamp
            );";
            db.Execute(createSpaces);

            // Таблица для досок (boards) внутри пространства
            string createBoards = @"
            CREATE TABLE IF NOT EXISTS Boards (
               id serial primary key,
               name varchar(50) not null,
               space_id int REFERENCES Spaces(id) ON DELETE CASCADE,
               owner_id int REFERENCES Users(id) ON DELETE CASCADE,
               created_at timestamp default current_timestamp
            );";
            db.Execute(createBoards);

            // Таблица для колонок внутри доски
            string createColumns = @"
            CREATE TABLE IF NOT EXISTS Columns (
               id serial primary key,
               board_id int REFERENCES Boards(id) ON DELETE CASCADE,
               title varchar(50) not null,
               created_by int REFERENCES Users(id) ON DELETE SET NULL,
               created_at timestamp default current_timestamp
            );";
            db.Execute(createColumns);

            // Таблица для задач в колонке
            string createTasks = @"
            CREATE TABLE IF NOT EXISTS Tasks (
               id serial primary key,
               column_id int REFERENCES Columns(id) ON DELETE CASCADE,
               title varchar(100) not null,
               description text,
               status varchar(20) not null check (status in ('open', 'in progress', 'done')),
               created_by int REFERENCES Users(id) ON DELETE SET NULL,
               assigned_to int REFERENCES Users(id) ON DELETE SET NULL,
               created_at timestamp default current_timestamp,
               updated_at timestamp default current_timestamp
            );";
            db.Execute(createTasks);

            // Таблица логов для отслеживания изменений в задачах
            string createTaskLogs = @"
            CREATE TABLE IF NOT EXISTS TaskLogs (
               id serial primary key,
               task_id int REFERENCES Tasks(id) ON DELETE CASCADE,
               user_id int REFERENCES Users(id) ON DELETE SET NULL,
               action varchar(50) not null,
               old_value text,
               new_value text,
               created_at timestamp default current_timestamp
            );";
            db.Execute(createTaskLogs);

            Console.WriteLine("Database created");

            // Если таблица пользователей пуста, заполним базу начальными данными.
            var userCount = db.ExecuteScalar<int>("SELECT COUNT(*) FROM Users");
            if (userCount == 0)
            {
                 // Создаём пользователя
                 db.Execute("INSERT INTO Users (username, password) VALUES (@Username, @Password)",
                            new { Username = "default", Password = HashPassword("default", GenerateSalt()) });
                 Console.WriteLine("Inserted default user");

                 // Допустим, после вставки пользователя его id равен 1.
                 // Создаём пространство для пользователя
                 db.Execute("INSERT INTO Spaces (name, user_id) VALUES (@Name, @UserId)",
                            new { Name = "My Space", UserId = 1 });
                 Console.WriteLine("Inserted default space");

                 // Создаём доску внутри пространства (space_id = 1) с owner_id пользователя 1
                 db.Execute("INSERT INTO Boards (name, space_id, owner_id) VALUES (@Name, @SpaceId, @OwnerId)",
                            new { Name = "My Board", SpaceId = 1, OwnerId = 1 });
                 Console.WriteLine("Inserted default board");

                 // Создаём колонку для доски (board_id = 1)
                 db.Execute("INSERT INTO Columns (board_id, title, created_by) VALUES (@BoardId, @Title, @CreatedBy)",
                            new { BoardId = 1, Title = "To Do", CreatedBy = 1 });
                 Console.WriteLine("Inserted default column");

                 // Создаём задачу в колонке (column_id = 1)
                 db.Execute("INSERT INTO Tasks (column_id, title, description, status, created_by, assigned_to) VALUES (@ColumnId, @title, @Description, @Status, @CreatedBy, @AssignedTo)",
                            new {
                                ColumnId = 1,
                                Title = "first start task",
                                Description = "This is a sample task.",
                                Status = "open",
                                CreatedBy = 1,
                                AssignedTo = 1
                            });
                 Console.WriteLine("Inserted default task");
            }
        }
        
    }
    
    public class UserModel
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}