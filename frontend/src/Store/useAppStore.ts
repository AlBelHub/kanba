import { create } from "zustand";
import { Board } from "../Types/types";

type AppStore = {
    userId: string;
    spaceId: string;
    boardId: string;
    currentBoard: Board | null;
  
    setUserId: (id: string) => void;
    setSpaceId: (id: string) => void;
    setBoardId: (id: string) => void;
    setBoard: (Board: Board | null) => void;
  };

  export const useAppStore = create<AppStore>((set) => ({
    userId: "NOT SET",
    spaceId: "NOT SET",
    boardId: "NOT SET",
    currentBoard: null,
  
    setUserId: (id) => set({ userId: id }),
    setSpaceId: (id) => set({ spaceId: id }),
    setBoardId: (id) => set({ boardId: id }),
    setBoard: (board) => set({ currentBoard: board}),
  }));