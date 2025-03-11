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
  boardId: number;
  position: number;
};

export type BoardsBodyProps = {
  name: string;
  space_id: number;
  owner_id: number;
}

export type TaskBodyProps = {
  column_id: number;
  board_id: number;
  title: string;
  description: string;
  status: string;
  position: number;
  created_by: number;
}

export type Board = {
  id: number;
  name: string;
  space_id: number;
  owner_id: number;
};
