import React from "react";
import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { Task } from "../Types/types";

interface TaskProps {
  task: Task;
  columnId: string;
  taskIndex: number;
  onTaskClick: any;
}

const TaskItem: React.FC<TaskProps> = ({ task, columnId, taskIndex, onTaskClick }) => {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useSortable({
    id: task.id,
    data: { columnId, taskIndex, type: "task" },
  });

  const style = {
    transform: CSS.Transform.toString(transform),
    width: "100%", // Задача занимает всю ширину колонки
    opacity: isDragging ? 0.5 : 1,
};

const handleTaskClick = (event: React.MouseEvent) => {
  onTaskClick(task, columnId); // Открытие FloatingWindow
};

  return (
    <div
      ref={setNodeRef}
      style={style}
      className="window task"
      onClick={(e) => handleTaskClick(e)}

>
      <div className="title-bar" 
      
      {...attributes}
      {...listeners}
      >
        <div className="title-bar-text">{task.title}</div>
        <div className="title-bar-controls">
          <button aria-label="Close" />
        </div>
      </div>
      <div className="window-body" 
      >
        <p>{task.id}</p>
        <p>{task.title}</p>
      </div>
    </div>
  );
};

export default TaskItem;