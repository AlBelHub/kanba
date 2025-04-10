using System.Data;
using System.Text;
using backend.Helpers;
using backend.Models;
using backend.Repositories;
using backend.Repositories.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Board = backend.Models.Board;

namespace backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

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
                    new string[] { }
                }
            });
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:5173",
                        "http://172.18.0.4:5173",
                        "http://kanba-frontend-1:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        builder.Services.AddControllers();
        
        builder.Services.AddTransient<IDbConnection>(_ =>
        {
            var connectionString = builder.Configuration.GetConnectionString("Postgres")
                                   ?? throw new InvalidOperationException("Postgres connection string is missing.");
            return new NpgsqlConnection(connectionString);
        });
        
        builder.Services.AddTransient<IUserRepository, UserRepository>();
        builder.Services.AddTransient<ISpaceRepository, SpaceRepository>();
        builder.Services.AddTransient<IBoardRepository, BoardRepository>();
        builder.Services.AddTransient<IColumnRepository, ColumnRepository>();
        builder.Services.AddTransient<ITaskRepository, TaskRepository>();
        builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();
        builder.Services.AddTransient<IUUIDProvider, UUIDProvider>();
        builder.Services.AddTransient<ITokenGenerator, TokenGenerator>();
        
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
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.UseCors("AllowFrontend");
        app.MapControllers();

        app.Use((context, next) =>
        {
            if (context.Request.Method == "OPTIONS")
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
                context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                return context.Response.WriteAsync("handled");
            }

            return next.Invoke();
        });

        app.UseRouting();

        // Configure the HTTP request pipeline.
        // if (app.Environment.IsDevelopment())
        // {
            app.UseSwagger();
            app.UseSwaggerUI();
        // }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        EnsureDatabaseCreated(app.Services.GetRequiredService<IDbConnection>(),
                    app.Services.GetRequiredService<IUserRepository>(), 
                    app.Services.GetRequiredService<ISpaceRepository>(),
                    app.Services.GetRequiredService<IBoardRepository>(),
                    app.Services.GetRequiredService<IColumnRepository>(),
                    app.Services.GetRequiredService<ITaskRepository>());

        app.Run();

     
        async void EnsureDatabaseCreated(
            IDbConnection db, 
            IUserRepository userRepository, 
            ISpaceRepository spaceRepository,
            IBoardRepository boardRepository,
            IColumnRepository columnRepository,
            ITaskRepository taskRepository)
        {
            // Создание таблицы пользователей
            string createUsers = @"
            CREATE TABLE IF NOT EXISTS Users (
               id UUID primary key,
               username varchar(40) not null,
               password varchar(255) not null
            );";
            db.Execute(createUsers);
            
            
            //Таблица для пространств (spaces) — личное рабочее пространство пользователя
            string createSpaces = @"
            CREATE TABLE IF NOT EXISTS Spaces (
               id UUID primary key,
               name varchar(50) not null,
               user_id UUID REFERENCES Users(id) ON DELETE CASCADE,
               created_at timestamp default current_timestamp
            );";
            db.Execute(createSpaces);

            // Таблица для досок (boards) внутри пространства
            string createBoards = @"
            CREATE TABLE IF NOT EXISTS Boards (
               id UUID primary key,
               name varchar(50) not null,
               space_id UUID REFERENCES Spaces(id) ON DELETE CASCADE,
               owner_id UUID REFERENCES Users(id) ON DELETE CASCADE,
               created_at timestamp default current_timestamp
            );";
            db.Execute(createBoards);

            // Таблица для колонок внутри доски
            string createColumns = @"
            CREATE TABLE IF NOT EXISTS Columns (
               id UUID primary key,
               board_id UUID REFERENCES Boards(id) ON DELETE CASCADE,
               title varchar(50) not null,
               position int not null,
               created_by UUID REFERENCES Users(id) ON DELETE SET NULL,
               created_at timestamp default current_timestamp
            );";
            db.Execute(createColumns);

            string createTasks = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                       id UUID primary key,
                       column_id UUID REFERENCES Columns(id) ON DELETE CASCADE,
                       board_id UUID REFERENCES Boards(id) ON DELETE CASCADE, -- Новое поле
                       title varchar(100) not null,
                       description text,
                       status varchar(20) not null check (status in ('open', 'in progress', 'done')),
                       position int not null,
                       created_by UUID REFERENCES Users(id) ON DELETE SET NULL,
                       assigned_to UUID REFERENCES Users(id) ON DELETE SET NULL,
                       created_at timestamp default current_timestamp,
                       updated_at timestamp default current_timestamp
                    );";
            db.Execute(createTasks);

            // Таблица логов для отслеживания изменений в задачах
            string createTaskLogs = @"
            CREATE TABLE IF NOT EXISTS TaskLogs (
               id UUID primary key,
               task_id UUID REFERENCES Tasks(id) ON DELETE CASCADE,
               user_id UUID REFERENCES Users(id) ON DELETE SET NULL,
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
                var user = await userRepository.RegisterUserAsync("admin", "admin");

                if (user == null)
                {
                    Console.WriteLine("User not registered");
                }
                
                Console.WriteLine("Inserted default user " + user.id + " " + user.username + " " + user.password);
                
                // Допустим, после вставки пользователя его id равен 1.
                // Создаём пространство для пользователя
                // db.Execute("INSERT INTO Spaces (name, user_id) VALUES (@Name, @UserId)",
                //     new { Name = "My Space", UserId = 1 });

                var space = await spaceRepository.CreateSpace(user.id, "default");

                if (space == null)
                {
                    Console.WriteLine("Space not created");
                }
                
                Console.WriteLine("Inserted default space " + space.Name + " " + space.Id );

                // Создаём доску внутри пространства (space_id = 1) с owner_id пользователя 1
                Board[] boards = new[]
                {
                    new Board { Name = "Web Development", SpaceId = space.Id, OwnerId = user.id },
                    new Board { Name = "Mobile App", SpaceId = space.Id, OwnerId = user.id },
                    new Board { Name = "Marketing Campaign", SpaceId = space.Id, OwnerId = user.id },
                    new Board { Name = "UX/UI Design", SpaceId = space.Id, OwnerId = user.id }
                };
                
                Random rand = new Random();

                foreach (var board in boards)
                {
                    var createdBoard = await boardRepository.CreateBoard(new BoardsProps {name = board.Name, space_id = board.SpaceId, owner_id = board.OwnerId});

                    if (createdBoard == null)
                    {
                        Console.WriteLine("Board not created");
                    }
                    
                    Console.WriteLine($"Inserted board: {board.Name}, {board.Id}");
                    
                    int columnCount = rand.Next(3, 6); // От 3 до 5 колонок на борде

                    for (int columnIndex = 1; columnIndex <= columnCount; columnIndex++)
                    {
                        // db.Execute(
                        //     "INSERT INTO Columns (board_id, title, created_by, position) VALUES (@BoardId, @Title, @CreatedBy, @Position)",
                        //     new
                        //     {
                        //         BoardId = createdBoard.Id, Title = $"Column {columnIndex}", CreatedBy = createdBoard.OwnerId,
                        //         Position = columnIndex
                        //     });

                        var column = await columnRepository.CreateColumn(new ColumnsProps(){ CreatedBy = user.id, Position = columnIndex, BoardId = createdBoard.Id , Title = $"from for {columnIndex}"});

                        int taskCount = rand.Next(5, 11); // От 5 до 10 задач на колонку

                        for (int taskIndex = 1; taskIndex <= taskCount; taskIndex++)
                        {
                            taskRepository.CreateTask(new TaskProps()
                            {
                                
                                column_id = column.Id,
                                board_id= createdBoard.Id,
                                title = $"Task {taskIndex} in Column {columnIndex}",
                                description = "This is a sample task.",
                                status = "open",
                                position= taskIndex,
                                created_by = user.id,

                            });                            

                            Console.WriteLine($"Inserted task {taskIndex} in column {columnIndex} on board {createdBoard.Id}");
                        }
                    }
                }

            }
        }

    }
}