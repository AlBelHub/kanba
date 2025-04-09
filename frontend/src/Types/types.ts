export type Task = {
  id: string;
  title: string;
  position: number;
};

export type Column = {
  id: string;
  title: string;
  position: number;
  tasks: Task[];
};

export type ColumnsBodyProps = {
  createdBy: number;
  title: string;
  boardId: string;
  position: number;
};

export type BoardsBodyProps = {
  name: string;
  space_id: string;
  owner_id: string;
}

export type TaskBodyProps = {
  column_id: string;
  board_id: string;
  title: string;
  description: string;
  status: string;
  position: number;
  created_by: string;
}

export type Board = {
  id: string;
  name: string;
  space_id: string;
  owner_id: string;
};

export type BoardsProps = {
  name?: string | null;     // nullable
  space_id: string;         // UUID формат
  owner_id: string;         // UUID формат
};

export type ColumnsProps = {
  createdBy: string;        // UUID формат
  title?: string | null;    // nullable
  boardId: string;          // UUID формат
  position: number;         // int32
};

export type TaskMoveRequest = {
  oldColumnId: string;      // UUID формат
  newColumnId: string;      // UUID формат
  taskOldPos: number;       // int32
  taskNewPos: number;       // int32
  boardId: string;          // UUID формат
  taskId: string;           // UUID формат
};

export type TaskProps = {
  column_id: string;        // UUID формат
  board_id: string;         // UUID формат
  title?: string | null;    // nullable
  description?: string | null; // nullable
  status?: string | null;   // nullable
  position: number;         // int32
  created_by: string;       // UUID формат
};

export type UserModel = {
  username?: string | null; // nullable
  password?: string | null; // nullable
};

export type Space = {
  id: string;
  name: string;
  user_id: string;
  created_at:string;
};

export type User = {
  id: string;
  username: string;
  password: string;
}

