import { create } from "zustand";
import { getUsers, getSpacesByUserId, getBoards } from "../Utils/Api";

type AppStore = {
    userId: string;
    spaceId: string;
    boardId: string;
    initialized: boolean;
  
    setUserId: (id: string) => void;
    setSpaceId: (id: string) => void;
    setBoardId: (id: string) => void;
  
    initialize: () => Promise<void>;
  };

  export const useAppStore = create<AppStore>((set) => ({
    userId: "2b5a7000-15fe-0196-b617-8e1f3d8349b7",
    spaceId: "2b777000-15fe-0196-98a5-c1efa37d8484",
    boardId: "2b837000-15fe-0196-5c21-a6f8938c929c",
    initialized: false,
  
    setUserId: (id) => set({ userId: id }),
    setSpaceId: (id) => set({ spaceId: id }),
    setBoardId: (id) => set({ boardId: id }),
  
    initialize: async () => {
      try {
        const users = await getUsers();
        const currentUser = users[0]; // FIX IT!
        const userId = currentUser.id;
  
        const spaces = await getSpacesByUserId(userId);
        const spaceId = spaces[0]?.id;
  
        const boards = await getBoards(spaceId);
        const boardId = boards[0]?.id;
  
        set({
          userId,
          spaceId,
          boardId,
          initialized: true,
        });
      } catch (error) {
        console.error("Ошибка инициализации стора:", error);
      }
    },
  }));