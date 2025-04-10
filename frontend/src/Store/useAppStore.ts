import { create } from "zustand";
import { Board } from "../Types/types";

type AppStore = {
  userId: string | null;
  spaceId: string;
  boardId: string;
  currentBoard: Board | null;
  token: string | null;

  setUserId: (id: string) => void;
  setSpaceId: (id: string) => void;
  setBoardId: (id: string) => void;
  setBoard: (Board: Board | null) => void;
  setToken: (token: string | null) => void;
};

export const useAppStore = create<AppStore>((set) => ({
  userId: localStorage.getItem("userId") || null,
  spaceId: "NOT SET",
  boardId: "NOT SET",
  currentBoard: null,
  token: localStorage.getItem("token") || null,

  setUserId: (userId) => {
    set({ userId });
    if (userId) {
      localStorage.setItem("userId", userId); // Сохраняем токен в localStorage
    } else {
      localStorage.removeItem("userId"); // Удаляем токен из localStorage, если его нет
    }
  },
  setSpaceId: (id) => set({ spaceId: id }),
  setBoardId: (id) => set({ boardId: id }),
  setBoard: (board) => set({ currentBoard: board }),
  setToken: (token) => {
    set({ token });
    if (token) {
      localStorage.setItem("token", token); // Сохраняем токен в localStorage
    } else {
      localStorage.removeItem("token"); // Удаляем токен из localStorage, если его нет
    }
  },
}));
