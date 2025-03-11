import {
  BoardsBodyProps,
  ColumnsBodyProps,
  TaskBodyProps,
} from "../Types/types";

const API_BASE_URL = "http://172.20.0.3:8080";

async function loginAndRetry(url: string, options: RequestInit) {
  try {
    const response = await fetch(`${API_BASE_URL}/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ username: "default", password: "default" }), // Подставь реальные данные
    });

    if (!response.ok) throw new Error("Ошибка входа");

    const { token: newToken } = await response.json();
    localStorage.setItem("token", newToken);

    // Повторяем оригинальный запрос с новым токеном
    return fetchData(url, options);
  } catch (error) {
    console.error("Не удалось войти:", error);
    throw error;
  }
}

export async function createColumn(props: ColumnsBodyProps) {
  const url = "/columns"; // URL для создания колонки
  const options: RequestInit = {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      BoardId: props.boardId,
      Title: props.title,
      CreatedBy: props.createdBy,
      Position: props.position,
    }),
  };

  try {
    const newColumn = await fetchData(url, options);
    console.log("Created column:", newColumn);
    return newColumn;
  } catch (error) {
    console.error("Failed to create column:", error);
    throw error;
  }
}

export async function createBoard(props: BoardsBodyProps) {
  const url = "/boards"; // URL для создания колонки
  const options: RequestInit = {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      name: props.name,
      space_id: props.space_id,
      owner_id: props.owner_id,
    }),
  };

  try {
    const newBoard = await fetchData(url, options);
    console.log("Created board:", newBoard);
    return newBoard;
  } catch (error) {
    console.error("Failed to create board:", error);
    throw error;
  }
}

//TODO: dodelat!
export async function createTask(props: TaskBodyProps) {
  const url = "/tasks"; // URL для создания задачи
  const options: RequestInit = {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      column_id: props.column_id, 
      board_id: props.board_id,
      title: props.title,
      description: props.description,
      status: props.status,
      position: props.position,
      created_by: props.created_by,
    }),
  };

  try {
    const newTask = await fetchData(url, options);
    console.log("Created task:", newTask);
    return newTask;
  } catch (error) {
    console.error("Failed to create board:", error);
    throw error;
  }
}

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

export default fetchData;
