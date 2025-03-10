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