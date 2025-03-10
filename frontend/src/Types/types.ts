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

export type Board = {
  id: number;
  name: string;
  space_id: number;
  owner_id: number;
}
