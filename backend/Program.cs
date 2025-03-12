using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:5173",
                        "http://172.20.0.4:5173",
                        "http://kanba-frontend-1:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        
        builder.Services.AddScoped<IDbConnection>(_ =>
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

        app.UseCors("AllowFrontend");

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

        //TODO: Выключить этот метод
        app.MapGet("/spaces", async (IDbConnection db) =>
        {
            var spaces = await db.QueryAsync("SELECT id, name, user_id FROM Spaces");
            return Results.Ok(spaces);
        });

        app.MapGet("/spaces/{user_id}", async (IDbConnection db, int userId) =>
        {
            var spaces = await db.QueryAsync("SELECT id, name, user_id FROM Spaces WHERE user_id = @user_id", new { user_id = userId });
            //return
        });

        app.MapGet("/boards/{space_id}", async (int space_id, IDbConnection db) =>
        {
            var boards = 
                await db.QueryAsync("SELECT id, name, space_id, owner_id FROM Boards WHERE space_id = @space_id", 
                    new { space_id = space_id });
            return Results.Ok(boards);
        });

        app.MapPost("/boards", async ([FromBody] BoardsProps props, IDbConnection db) =>
        {
            var query = @"
                    INSERT INTO Boards (name, space_id, owner_id) VALUES (@name,@space_id,@owner_id)
                    RETURNING id, name, space_id, owner_id";
            
            var newBoard = await db.QueryFirstOrDefaultAsync(query, props);
            
            if (newBoard != null)
            {
                return Results.Ok(newBoard);
            }
            else
            {
                return Results.BadRequest("Что-то пошло не так при создании колонки");
            }
        });
        
        app.MapGet("/columns/{board_id}", [Authorize] async (int board_id, IDbConnection db) =>
        {
            var columns = 
                await db.QueryAsync("SELECT id, board_id, title, created_by FROM Columns WHERE board_id = @board_id", 
                new { board_id = board_id });
            return Results.Ok(columns);
        });

        //TODO: ГЕНЕРИРОВАТЬ НОРМАЛЬНЫЙ ID
        app.MapPost("/columns", async ([FromBody] ColumnsProps props , IDbConnection db) =>
        {
            
            var query = @"
                    INSERT INTO Columns (board_id, title, created_by, position) VALUES (@BoardId, @Title,@CreatedBy, @Position)
                    RETURNING id, board_id, title, created_by, position";
            
            var newColumn = await db.QueryFirstOrDefaultAsync(query, props);
            
            if (newColumn != null)
            {
                return Results.Ok(newColumn);
            }
            else
            {
                return Results.BadRequest("Что-то пошло не так при создании колонки");
            }
        });

        ///POST
        /// /columnMove?ColumnId=1&OldPosition=2&NewPosition=3&SpaceId=1&BoardId=1
        app.MapGet("/columnMove", async (int ColumnId, int OldPosition, int NewPosition, int SpaceId, int BoardId, IDbConnection db) =>
        {
            if (db.State != ConnectionState.Open)
            {
                db.Open();
            }

            using var transaction = db.BeginTransaction();

            try
            {
                // 1. Проверяем, существует ли колонка с заданным ColumnId
                var columnExists = await db.ExecuteScalarAsync<bool>(
                    "SELECT EXISTS (SELECT 1 FROM columns WHERE id = @ColumnId AND board_id = @BoardId)",
                    new { ColumnId, BoardId });

                if (!columnExists)
                {
                    return Results.NotFound(new { message = "Column not found in the specified board" });
                }

                // 2. Если колонка переместилась вниз
                if (OldPosition < NewPosition)
                {
                    await db.ExecuteAsync(
                        "UPDATE columns SET position = position - 1 WHERE position > @OldPosition AND position <= @NewPosition AND board_id = @BoardId",
                        new { OldPosition, NewPosition, BoardId }, transaction);
                }
                // 3. Если колонка переместилась вверх
                else if (OldPosition > NewPosition)
                {
                    await db.ExecuteAsync(
                        "UPDATE columns SET position = position + 1 WHERE position < @OldPosition AND position >= @NewPosition AND board_id = @BoardId",
                        new { OldPosition, NewPosition, BoardId }, transaction);
                }

                // 4. Обновляем позицию перемещённой колонки
                await db.ExecuteAsync(
                    "UPDATE columns SET position = @NewPosition WHERE id = @ColumnId AND board_id = @BoardId",
                    new { ColumnId, NewPosition, BoardId }, transaction);

                transaction.Commit();
                return Results.Ok(new { message = "Column position updated successfully" });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Results.BadRequest(new { message = "Failed to update column position", error = ex.Message });
            }
        });


        //Найти все таски в борде, поменять местами
        
        
        app.MapGet("/tasks", async (IDbConnection db) =>
        {
            var tasks = await db.QueryAsync("SELECT id, column_id, title, description, status, created_by, assigned_to FROM Tasks");
            
            return Results.Ok(tasks);
        });

        app.MapPost("/tasks", async ([FromBody] TaskProps props,IDbConnection db) =>
        {
            var sql = @"
                INSERT INTO Tasks (
                    column_id, 
                    board_id, 
                    title, 
                    description, 
                    status, 
                    position,
                    created_by
                ) VALUES (
                    @column_id,
                    @board_id,
                    @title, 
                    @description, 
                    @status, 
                    @position,
                    @created_by 
                    ) 
                RETURNING id::TEXT || 't' AS id ,column_id,board_id,title,description,status,position,created_by";
            
            var newTask = await db.QueryFirstOrDefaultAsync(sql, props);

            if (newTask != null)
            {
                return Results.Ok(newTask);
            }
            else
            {
                return Results.BadRequest("Что-то пошло не так");
            }
        });

        
                //TODD: POST and BODY
        app.MapGet("/taskMove", async (int OldColumnId, int NewColumnId, int TaskOldPos, int TaskNewPos, int BoardId, int TaskId, IDbConnection db) =>
        {
            if (db.State != ConnectionState.Open)
            {
                db.Open();
            }

            using var transaction = db.BeginTransaction();
            try
            {
                if (OldColumnId == NewColumnId)
                {
                    // Если перемещаем внутри одной колонки
                    if (TaskOldPos < TaskNewPos)
                    {
                        // Двигаем вверх (уменьшаем номера)
                        await db.ExecuteAsync(
                            "UPDATE tasks SET position = position - 1 WHERE column_id = @ColumnId AND position > @TaskOldPos AND position <= @TaskNewPos",
                            new { ColumnId = OldColumnId, TaskOldPos, TaskNewPos }, transaction);
                    }
                    else
                    {
                        // Двигаем вниз (увеличиваем номера)
                        await db.ExecuteAsync(
                            "UPDATE tasks SET position = position + 1 WHERE column_id = @ColumnId AND position >= @TaskNewPos AND position < @TaskOldPos",
                            new { ColumnId = OldColumnId, TaskOldPos, TaskNewPos }, transaction);
                    }

                    // Обновляем позицию задачи
                    await db.ExecuteAsync(
                        "UPDATE tasks SET position = @TaskNewPos WHERE id = @TaskId",
                        new { TaskNewPos, TaskId }, transaction);
                }
                else
                {
                    // Освобождаем позицию в старой колонке
                    await db.ExecuteAsync(
                        "UPDATE tasks SET position = position - 1 WHERE column_id = @OldColumnId AND position > @TaskOldPos",
                        new { OldColumnId, TaskOldPos }, transaction);

                    // Вставляем задачу в новую колонку
                    await db.ExecuteAsync(
                        "UPDATE tasks SET column_id = @NewColumnId, position = @TaskNewPos WHERE id = @TaskId",
                        new { NewColumnId, TaskNewPos, TaskId }, transaction);

                    // Сдвигаем задачи вниз в новой колонке
                    await db.ExecuteAsync(
                        "UPDATE tasks SET position = position + 1 WHERE column_id = @NewColumnId AND position >= @TaskNewPos AND id <> @TaskId",
                        new { NewColumnId, TaskNewPos, TaskId }, transaction);
                }

                transaction.Commit();
                return Results.Ok(new { message = "Task moved successfully" });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                    //!!!!!
                return Results.Problem(ex.Message);
            }
        });



//TODD: POST and BODY
        app.MapGet("/columns_with_tasks/{spaceId}/{boardId}", async (int spaceId, int boardId, IDbConnection db) =>
        {
            var sql = @"
                SELECT 
                    c.id AS ColumnId,
                    c.title AS ColumnTitle,
                    c.position AS ColumnPosition,
                    t.id AS TaskId,
                    t.title AS TaskTitle,
                    t.board_id AS BoardId,
                    t.position AS TaskPosition
                FROM columns c
                LEFT JOIN tasks t ON c.id = t.column_id
                WHERE c.board_id = @boardId
                AND EXISTS (
                    SELECT 1 FROM boards b 
                    WHERE b.id = @boardId 
                    AND b.space_id = @spaceId
                )";

            var rows = await db.QueryAsync<ColumnTaskRow>(sql, new { spaceId, boardId });
            
            // Группируем задачи по колонкам
            var columns = rows.GroupBy(r => new { r.ColumnId, r.ColumnTitle, r.ColumnPosition })
                .OrderBy(g => g.Key.ColumnPosition)
                .Select(g => new Column
                {
                    Id = g.Key.ColumnId.ToString() + "c", //Без добавления типа начнутся коллизии между колонками и тасками
                    Title = g.Key.ColumnTitle,
                    Position = g.Key.ColumnPosition,
                    Tasks = g.Where(t => t.TaskId.HasValue)
                        .OrderBy(t => t.TaskPosition)
                        .Select(t => new TaskItem
                        {
                            Id = t.TaskId?.ToString() + "t", //Без добавления типа начнутся коллизии между колонками и тасками
                            Position = t.TaskPosition,
                            Title = t.TaskTitle
                        }).ToList()
                }).ToList();

            return Results.Ok(columns);
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
               position int not null,
               created_by int REFERENCES Users(id) ON DELETE SET NULL,
               created_at timestamp default current_timestamp
            );";
            db.Execute(createColumns);

            string createTasks = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                       id serial primary key,
                       column_id int REFERENCES Columns(id) ON DELETE CASCADE,
                       board_id int REFERENCES Boards(id) ON DELETE CASCADE, -- Новое поле
                       title varchar(100) not null,
                       description text,
                       status varchar(20) not null check (status in ('open', 'in progress', 'done')),
                       position int not null,
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
                 var boards = new[]
                 {
                     new { Name = "Web Development", SpaceId = 1, OwnerId = 1 },
                     new { Name = "Mobile App", SpaceId = 1, OwnerId = 1 },
                     new { Name = "Marketing Campaign", SpaceId = 1, OwnerId = 1 },
                     new { Name = "UX/UI Design", SpaceId = 1, OwnerId = 1 }
                 };

                 Random rand = new Random();

                foreach (var board in boards)
                {
                    // Вставляем доску, если её нет
                    db.Execute(@"
                        INSERT INTO Boards (name, space_id, owner_id) 
                        SELECT @Name, @SpaceId, @OwnerId
                        WHERE NOT EXISTS (
                            SELECT 1 FROM Boards 
                            WHERE name = @Name AND space_id = @SpaceId
                        )",
                        board);

                    Console.WriteLine($"Inserted board: {board.Name}");

                    // Получаем ID доски
                    int boardId = db.ExecuteScalar<int>(
                        "SELECT id FROM Boards WHERE name = @Name AND space_id = @SpaceId",
                        new { board.Name, board.SpaceId });

                    int columnCount = rand.Next(3, 6); // От 3 до 5 колонок на борде

                    for (int columnIndex = 1; columnIndex <= columnCount; columnIndex++)
                    {
                        db.Execute("INSERT INTO Columns (board_id, title, created_by, position) VALUES (@BoardId, @Title, @CreatedBy, @Position)",
                            new { BoardId = boardId, Title = $"Column {columnIndex}", CreatedBy = 1, Position = columnIndex });

                        // Получаем ID созданной колонки
                        int columnId = db.ExecuteScalar<int>(
                            "SELECT id FROM Columns WHERE board_id = @BoardId AND title = @Title",
                            new { BoardId = boardId, Title = $"Column {columnIndex}" });

                        int taskCount = rand.Next(5, 11); // От 5 до 10 задач на колонку

                        for (int taskIndex = 1; taskIndex <= taskCount; taskIndex++)
                        {
                            db.Execute(@"
                                INSERT INTO Tasks (
                                    column_id, 
                                    board_id, 
                                    title, 
                                    description, 
                                    status, 
                                    position,
                                    created_by, 
                                    assigned_to
                                ) VALUES (
                                    @ColumnId,
                                    @BoardId,
                                    @Title, 
                                    @Description, 
                                    @Status, 
                                    @Position,
                                    @CreatedBy, 
                                    @AssignedTo
                                )", new {
                                ColumnId = columnId,
                                BoardId = boardId,
                                Title = $"Task {taskIndex} in Column {columnIndex}",
                                Description = "This is a sample task.",
                                Status = "open",
                                Position = taskIndex,
                                CreatedBy = 1,
                                AssignedTo = 1
                            });

                            Console.WriteLine($"Inserted task {taskIndex} in column {columnIndex} on board {boardId}");
        }
    }
}

            }
        }
        
    }
    
    public class UserModel
    {
        public string username { get; set; }
        public string password { get; set; }
    }
    
    public class ColumnTaskRow
    {
        public int ColumnId { get; set; }
        public string ColumnTitle { get; set; }
        public string ColumnPosition { get; set; } // Позиция колонки
        public int? TaskId { get; set; }
        public string TaskTitle { get; set; }
        public int BoardId { get; set; }
        public string TaskPosition { get; set; } // Позиция задачи
    }

    public class Column
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string? Position { get; set; } // Позиция колонки
        public List<TaskItem> Tasks { get; set; }
    }

    public class ColumnsProps
    {
        public int CreatedBy { get; set; }
        public string Title { get; set; }
        public int BoardId { get; set; }
        public int Position { get; set; }
    }

    public class BoardsProps
    {
        public string name { get; set; }
        public int space_id { get; set; }
        public int owner_id { get; set; }
    }

    public class TaskProps
    {
        public int column_id { get; set; }
        public int board_id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public int position { get; set; }
        public int created_by { get; set; }
    }
    
    public class TaskItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string? Position { get; set; } // Позиция задачи
    }

}