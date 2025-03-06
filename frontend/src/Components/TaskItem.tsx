import React from "react";
import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { Task } from "../Types/types";

interface TaskProps {
  task: Task;
  columnId: string;
  taskIndex: number;
}

const TaskItem: React.FC<TaskProps> = ({ task, columnId, taskIndex }) => {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useSortable({
    id: task.id,
    data: { columnId, taskIndex, type: "task" },
  });

  const style = {
    transform: CSS.Transform.toString(transform),
    width: "100%", // Задача занимает всю ширину колонки
    opacity: isDragging ? 0.5 : 1,
};

  return (
    <div
      ref={setNodeRef}
      style={style}
      {...attributes}
      {...listeners}
      className="window task"
    >
      <div className="title-bar">
        <div className="title-bar-text">{task.title}</div>
        <div className="title-bar-controls">
          <button aria-label="Minimize" />
          <button aria-label="Maximize" />
          <button aria-label="Close" />
        </div>
      </div>
      <div className="window-body">
        <p>ЗАДАЧА {taskIndex + 1}</p>
        <p>АВТОР</p>
      </div>
    </div>
  );
};

export default TaskItem;