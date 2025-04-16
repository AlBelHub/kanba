import {
  // Дополнительно предполагаемые типы — если они у вас есть:
  UserModel,        // для /api/Auth/login и /api/Auth/register
  BoardsProps,      // для /api/Boards/createBoard (вместо BoardsBodyProps, если требуется)
  ColumnsProps,     // для /api/Columns (создание колонки)
  TaskProps,        // для /createTask
  TaskMoveRequest,  // для /moveTask
  Board,
  Column,
  Task,
  Space,
  User,
  TaskWithUsersDto,
} from "../Types/types";

const API_BASE_URL = "http://172.18.0.3:8080/api";

/**
 * Универсальная функция для выполнения запросов с обработкой авторизации.
 * При получении статуса 401 происходит вызов loginAndRetry.
 */
async function fetchData(url: string, options: RequestInit = {}): Promise<any> {
  const token = localStorage.getItem("token");

  const headers = {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...options.headers,
  };

  const response = await fetch(`${API_BASE_URL}${url}`, {
    ...options,
    headers,
  });

  if (response.status === 401) {
    return loginAndRetry(url, options);
  }

  if (!response.ok) {
    throw new Error(`Ошибка запроса: ${response.status}`);
  }

  return response.json();
}

/**
 * Выполняет вход по умолчанию и повторяет исходный запрос.
 * Если нужно использовать другие учетные данные — уточните, какой тип или параметры использовать.
 */
async function loginAndRetry(url: string, options: RequestInit) {
  try {
    // Здесь используется схема UserModel для входа.
    const loginCredentials: UserModel = {
      username: "default",
      password: "default",
      // Если структура UserModel иная – уточните
    };

    const response = await fetch(`${API_BASE_URL}/Auth/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(loginCredentials),
    });

    if (!response.ok) throw new Error("Ошибка входа");

    const { token: newToken } = await response.json();
    localStorage.setItem("token", newToken);

    // Повторяем исходный запрос с новым токеном
    return fetchData(url, options);
  } catch (error) {
    console.error("Не удалось войти:", error);
    throw error;  
  }
}

/* ======================= Методы API ======================= */

/**
 * /api/Auth/login  
 * POST — Аутентификация пользователя.
 * @param credentials данные для входа типа UserModel
 */
export async function loginUser(credentials: UserModel): Promise<string> {
  const url = "/Auth/login";
  const options: RequestInit = {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(credentials),
  };

  try {
    const userData = await fetchData(url, options);
    console.log("Вход выполнен успешно:", userData);
    return userData;
  } catch (error) {
    console.error("Ошибка входа:", error);
    throw error;
  }
}

/**
 * /api/Auth/register  
 * POST — Регистрация пользователя.
 * @param newUser данные для регистрации типа UserModel  
 */
export async function registerUser(newUser: UserModel): Promise<UserModel> {
  const url = "/Auth/register";
  const options: RequestInit = {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(newUser),
  };

  try {
    const registeredUser = await fetchData(url, options);
    console.log("Пользователь зарегистрирован:", registeredUser);
    return registeredUser;
  } catch (error) {
    console.error("Ошибка регистрации:", error);
    throw error;
  }
}

/**
 * /api/Auth/validate_token  
 * GET — Валидация токена.  
 * Если нужно передавать какой-либо параметр — уточните, пожалуйста.
 */
export async function validateToken(): Promise<any> {
  const url = "/Auth/validate_token";
  const options: RequestInit = {
    method: "GET",
    headers: { "Content-Type": "application/json" },
  };

  try {
    const result = await fetchData(url, options);
    console.log("Результат валидации токена:", result);
    return result;
  } catch (error) {
    console.error("Ошибка валидации токена:", error);
    throw error;
  }
}

/**
 * /api/Boards/getBoards/{space_id}  
 * GET — Получает список досок по идентификатору пространства.
 */
export async function getBoards(spaceId: string): Promise<Board[]> {
  const url = `/Boards/getBoards/${spaceId}`;
  console.log(url)
  try {
    const boards = await fetchData(url);
    console.log("Получены доски:", boards);
    return boards;
  } catch (error) {
    console.error("Ошибка получения досок:", error);
    throw error;
  }
}

/**
 * /api/Boards/createBoard  
 * POST — Создает доску.
 */
export async function createBoard(board: BoardsProps): Promise<Board> {
  const url = "/Boards/createBoard";
  const options: RequestInit = {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(board),
  };

  try {
    const newBoard = await fetchData(url, options);
    console.log("Доска создана:", newBoard);
    return newBoard;
  } catch (error) {
    console.error("Ошибка создания доски:", error);
    throw error;
  }
}

/**
 * /api/Columns/getByBoardId/{boardId}  
 * GET — Получает список колонок по идентификатору доски.
 */
export async function getColumnsByBoardId(boardId: string): Promise<Column[]> {
  const url = `/Columns/getByBoardId/${boardId}`;
  try {
    const columns = await fetchData(url);
    console.log("Получены колонки:", columns);
    return columns;
  } catch (error) {
    console.error("Ошибка получения колонок:", error);
    throw error;
  }
}

/**
 * /api/Columns  
 * POST — Создает колонку.
 */
export async function createColumn(column: ColumnsProps): Promise<Column> {
  const url = "/Columns";
  console.log(column)
  const options: RequestInit = {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(column),
  };

  try {
    const newColumn = await fetchData(url, options);
    console.log("Колонка создана:", newColumn);
    return newColumn;
  } catch (error) {
    console.error("Ошибка создания колонки:", error);
    throw error;
  }
}

/**
 * /api/Columns/moveColumn  
 * PUT — Перемещает колонку.  
 * Параметры передаются в query-параметрах:
 * - columnId (uuid)
 * - oldPosition (int32)
 * - newPosition (int32)
 * - boardId (uuid)
 */
export async function moveColumn(params: {
  columnId: string;
  oldPosition: number;
  newPosition: number;
  boardId: string;
}): Promise<any> {
  // Формирование query-параметров
  const queryParams = new URLSearchParams({
    columnId: params.columnId,
    oldPosition: params.oldPosition.toString(),
    newPosition: params.newPosition.toString(),
    boardId: params.boardId,
  });

  const url = `/Columns/moveColumn?${queryParams.toString()}`;
  const options: RequestInit = {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
  };

  try {
    const result = await fetchData(url, options);
    console.log("Колонка перемещена:", result);
    return result;
  } catch (error) {
    console.error("Ошибка перемещения колонки:", error);
    throw error;
  }
}

/**
 * /api/Columns/getColumnsAndTasks/{spaceId}/{boardId}  
 * GET — Получает список колонок и задач для доски в указанном пространстве.
 */
export async function getColumnsAndTasks(
  spaceId: string,
  boardId: string
): Promise<Column[]> {
  const url = `/Columns/getColumnsAndTasks/${spaceId}/${boardId}`;
  try {
    const data = await fetchData(url);
    console.log("Получены колонки и задачи:", data);
    return data;
  } catch (error) {
    console.error("Ошибка получения колонок и задач:", error);
    throw error;
  }
}

/**
 * /api/Space/getSpaces  
 * GET — Получает список пространств.
 */
export async function getSpaces(): Promise<Space[]> {
  const url = "/Space/getSpaces";
  try {
    const spaces = await fetchData(url);
    console.log("Получены пространства:", spaces);
    return spaces;
  } catch (error) {
    console.error("Ошибка получения пространств:", error);
    throw error;
  }
}

/**
 * /api/Space/getSpacesNyUserId/{userId}  
 * GET — Получает список пространств для указанного пользователя.
 */
export async function getSpacesByUserId(userId: string): Promise<Space[]> {
  const url = `/Space/getSpacesNyUserId/${userId}`;
  try {
    const spaces = await fetchData(url);
    console.log("Получены пространства по userId:", spaces);
    return spaces;
  } catch (error) {
    console.error("Ошибка получения пространств по userId:", error);
    throw error;
  }
}

/**
 * /getTasks  
 * GET — Получает список задач.
 */
export async function getTasks(): Promise<Task[]> {
  const url = "/Task/getTasks";
  try {
    const tasks = await fetchData(url);
    console.log("Получены задачи:", tasks);
    return tasks;
  } catch (error) {
    console.error("Ошибка получения задач:", error);
    throw error;
  }
}

export async function getTaskDetail(id: string) {
  const url = `/Task/getTaskDetail/${id}`
  const options: RequestInit = {
    method: "GET",
    headers: { "Content-Type": "application/json" },
  };

  try {
    const taskDetail: TaskWithUsersDto = await fetchData(url, options);
    console.log("Получены данные о задаче:", taskDetail);
    return taskDetail;
  } catch (error) {
    console.error("Ошибка получения данных", error);
    throw error;
  }
}

/**
 * /createTask  
 * POST — Создает задачу.
 */
export async function createTask(task: TaskProps): Promise<Task> {
  const url = "/Task/createTask";
  const options: RequestInit = {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(task),
  };

  try {
    const newTask = await fetchData(url, options);
    console.log("Задача создана:", newTask);
    return newTask;
  } catch (error) {
    console.error("Ошибка создания задачи:", error);
    throw error;
  }
}

/**
 * /updateTask  
 * PUT — Обновляет задачу.
 */
export async function updateTask(taskId: string, update: {
  title: string;
  description?: string;
  status: string;
  position: number;
  assigned_to?: string;
}): Promise<boolean> {
  const url = `/Task/updateTask?id=${taskId}`;
  const options: RequestInit = {
    method: "PUT", // можно заменить на POST, если API требует
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(update),
  };

  try {
    const result = await fetchData(url, options);
    console.log("Задача обновлена:", result);
    return result === true;
  } catch (error) {
    console.error("Ошибка обновления задачи:", error);
    throw error;
  }
}

/**
 * /deleteTask  
 * DELETE — Удаляет задачу.
 */
export async function deleteTask(taskId: string): Promise<boolean> {
  const url = `/Task/deleteTask?id=${taskId}`;
  const options: RequestInit = {
    method: "DELETE",
  };

  try {
    const result = await fetchData(url, options);
    console.log("Задача удалена:", result);
    return result === true;
  } catch (error) {
    console.error("Ошибка удаления задачи:", error);
    throw error;
  }
}

/**
 * /moveTask  
 * POST — Перемещает задачу.
 */
export async function moveTask(moveReq: TaskMoveRequest): Promise<any> {
  const url = "/Task/moveTask";
  const options: RequestInit = {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(moveReq),
  };

  try {
    const result = await fetchData(url, options);
    console.log("Задача перемещена:", result);
    return result;
  } catch (error) {
    console.error("Ошибка перемещения задачи:", error);
    throw error;
  }
}

/**
 * /api/User/getUsers  
 * GET — Получает список пользователей.
 */
export async function getUsers(): Promise<User[]> {
  const url = "/User/getUsers";
  try {
    const users = await fetchData(url);
    console.log("Получены пользователи:", users);
    return users;
  } catch (error) {
    console.error("Ошибка получения пользователей:", error);
    throw error;
  }
}

/**
 * /api/User/getUserId  
 * GET — Получить id юзера.
 */
export async function getUserId(username: string): Promise<string> {
  const url = `/User/getUserId/${username}`;
  try {
    const users = await fetchData(url);
    console.log("Получены пользователи:", users);
    return users;
  } catch (error) {
    console.error("Ошибка получения пользователей:", error);
    throw error;
  }
}


