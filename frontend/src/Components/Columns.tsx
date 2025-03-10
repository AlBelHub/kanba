// Columns.tsx
import React from "react";
import { useDroppable } from "@dnd-kit/core";
import { SortableContext, useSortable } from "@dnd-kit/sortable";
import { Column, Task } from "../Types/types";
import { CSS } from "@dnd-kit/utilities";
import TaskItem from "./TaskItem";

interface ColumnProps {
  colId: string;
  colName: string;
  tasks: Task[];
  setColumns: React.Dispatch<React.SetStateAction<Column[]>>;
}

export default function Columns({ colId, colName, tasks, setColumns }: ColumnProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging
  } = useSortable({
    id: colId,
    data: { type: "column" },
  });

  const { setNodeRef: droppableRef } = useDroppable({
    id: colId,
    data: { columnId: colId },
  });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    display: "block",
    verticalAlign: "top",
    opacity: isDragging ? 0.5 : 1,
    cursor: "grab",
    minHeight: "100rem", //Не должно так быть! 
  };

  const handleAddTask = () => {
    const newTask: Task = {
      id: `task-${Date.now()}`,
      title: 'Создано с кнопки!',
    }
    
    setColumns((prev) => (
      prev.map((col) => col.id === colId ? { ...col,tasks: [...col.tasks, newTask] } : col )
    ))
  }

  return (
    <div 
      ref={(node) => {
        setNodeRef(node); // Для сортировки колонок
        droppableRef(node); // Для дропа задач
      }}
      style={style}
    >
      <div className="window column">
        <div className="title-bar" {...attributes} {...listeners}>
          <div className="title-bar-text">{colName}</div>
          <div className="title-bar-controls">
            <button aria-label="Minimize" />
            <button aria-label="Maximize" />
            <button aria-label="Close" />
          </div>
        </div>
        <div className="window-body body-settings">
          <SortableContext items={tasks.map(task => task.id)}>
            {tasks.map((task, taskIndex) => (
              <TaskItem
                key={task.id}
                task={task}
                columnId={colId}
                taskIndex={taskIndex}
              />
            ))}
          </SortableContext>
          <button className="testTaskAdd hover_darkgray" onClick={handleAddTask}>Добавить задачу!</button>
        </div>
      </div>
    </div>
  );
}