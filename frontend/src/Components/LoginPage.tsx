import React, { useState } from "react";
import { redirect, useNavigate } from "react-router";
import { useAppStore } from "../Store/useAppStore";
import { User, UserModel } from "../Types/types";
import { getSpacesByUserId, getUserId, loginUser } from "../Utils/Api";

export default function LoginPage() {
  const [password, setPassword] = useState("");
  const [username, setUsername] = useState("");
  const [error, setError] = useState<string | null>(null);
  const { setUserId, setToken } = useAppStore();
  const navigate = useNavigate();

  const handleLogin = async () => {
    try {
      const { token } = await loginUser({ username, password });
      setToken(token);

      const userId = await getUserId(username);
      setUserId(userId);

      if (token && userId) {

        //TODO
        const space = await getSpacesByUserId(userId).then(data => data[0])

        if (space?.id) {
          navigate(`/${space.id}`);
        } else {
          setError("Пространство не найдено для этого пользователя.");
        }
      }
    } catch {
      setError("error");
    }
  };

  return (
    <div>
      <h4>LOGIN FORM</h4>
      <input
        type="password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        placeholder="Enter password"
      />
      <input
        type="text"
        value={username}
        onChange={(e) => setUsername(e.target.value)}
        placeholder="Enter username"
      />
      <button onClick={handleLogin}>Login</button>
      {error && <p>{error}</p>}
    </div>
  );
}
