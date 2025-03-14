// Columns.tsx
import React from "react";
import { useDroppable } from "@dnd-kit/core";
import { SortableContext, useSortable } from "@dnd-kit/sortable";
import { Column, Task } from "../Types/types";
import { CSS } from "@dnd-kit/utilities";
import TaskItem from "./TaskItem";
import { createTask } from "../Utils/Api";

interface ColumnProps {
  colId: string;
  colName: string;
  tasks: Task[];
  setColumns: React.Dispatch<React.SetStateAction<Column[]>>;
  onTaskClick: any
}

export default function Columns({ colId, colName, tasks, setColumns, onTaskClick }: ColumnProps) {
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

  //TODO: получать с бека только id
  const handleAddTask = async () => {
    try {
      const newTask = await createTask({
        board_id: 1, 
        column_id: parseInt(colId),
        title: "New Task", // заменить на пользовательский ввод
        description: "Task description",
        status: "open",
        position: !tasks || tasks.length === 0 ? 1 : Math.max(...tasks.map(task => task.position), 0) + 1,
        created_by: 1,
      });
  
      setColumns((prev) =>
        prev.map((col) =>
          col.id === colId ? { ...col, tasks: [...col.tasks, newTask] } : col
        )
      );
    } catch (error) {
      console.error("Ошибка при создании задачи:", error);
    }
  };

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
                onTaskClick={onTaskClick}
              />
            ))}
          </SortableContext>

          <button className="testTaskAdd hover_darkgray" onClick={handleAddTask} >Добавить задачу!</button>
        
        </div>
      </div>
    </div>
  );
}