import { useEffect, useRef, useState } from "react";
import {
  DndContext,
  DragEndEvent,
  DragOverlay,
  DragStartEvent,
} from "@dnd-kit/core";
import { SortableContext, arrayMove } from "@dnd-kit/sortable";
import { Column,} from "./Types/types";
import "98.css";
import "./App.css";
import Columns from "./Components/Columns";
import TaskItem from "./Components/TaskItem";
import Sidebar from "./Sidebar";
import fetchData from "./Utils/Api";

function App() {

  const TEST_DATA__SPACE_ID = 1;
  const TEST_DATA__BOARD_ID = 1;


  const scrollRef = useRef<HTMLDivElement | null>(null);
  const [columns, setColumns] = useState<Column[]>([
    {
      id: "todo",
      title: "Видать",
      position: 1,
      tasks: [{ id: "task-1", title: "Первая задача", position: 1 }],
    },
    {
      id: "done",
      title: "база",
      position: 2,
      tasks: [{ id: "task-3", title: "Третья задача", position: 2 }],
    },
    {
      id: "in-progress",
      title: "не рядом",
      position: 3,
      tasks: [{ id: "task-2", title: "Вторая задача", position: 3 }],
    },
  ]);

  const [activeTask, setActiveTask] = useState<any>(null);
  const [activeColumn, setActiveColumn] = useState<any>(null);


  useEffect(() => {
  
    fetchData(`/columns_with_tasks/${TEST_DATA__SPACE_ID}/${TEST_DATA__BOARD_ID}`).then(data => setColumns(data));

  }, []);
  
  useEffect(() => {
    const handleWheel = (event: WheelEvent) => {
      if (scrollRef.current) {
        event.preventDefault();
        scrollRef.current.scrollLeft += event.deltaY;
      }
    };

    const scrollContainer = scrollRef.current;
    if (scrollContainer) {
      scrollContainer.addEventListener("wheel", handleWheel, {
        passive: false,
      });
    }

    return () => {
      if (scrollContainer) {
        scrollContainer.removeEventListener("wheel", handleWheel);
      }
    };
  }, []);
  const handleDragStart = (event: DragStartEvent) => {
    const { active } = event;
    if (active.data.current?.type === "task") {
      const column = columns.find(
        (col) => col.id === active.data.current?.columnId
      );
      const task = column?.tasks.find((t) => t.id === active.id);
      setActiveTask({ ...task, columnId: active.data.current?.columnId });
      setActiveColumn(null);
    } else if (active.data.current?.type === "column") {
      const column = columns.find((col) => col.id === active.id);
      setActiveColumn(column);
      setActiveTask(null);
    }
  };
  const handleAddColumn = () => {
    const newColumn: Column = {
      id: `col-${columns.length + 1}`, // Уникальный ID
      title: `New Column ${columns.length + 1}`,
      position: columns[columns.length - 1].position + 1,
      tasks: [],
    };
    setColumns([...columns, newColumn]);
  };
  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;

    if (!over) {
      console.log("No over target");
      setActiveTask(null);
      setActiveColumn(null);
      return;
    }

    console.log("Active:", active.id, active.data.current?.type);
    console.log("Over:", over.id, over.data.current?.type);

    // Перетаскивание колонок
    if (active.data.current?.type === "column") {
      const oldIndex = columns.findIndex((col) => col.id === active.id);
      const oldIndexReal = columns[oldIndex].id;
      const newIndex = columns.findIndex((col) => col.id === over.id);
    
      if (oldIndex === -1 || newIndex === -1 || oldIndex === newIndex) {
        console.log("No reorder needed or invalid indices");
        return;
      }
    
      let updatedColumns = arrayMove(columns, oldIndex, newIndex);
      
      // Пересчёт позиции у всех колонок
      updatedColumns = updatedColumns.map((col, index) => ({
        ...col,
        position: index + 1, // Устанавливаем корректные позиции
      }));
    
      setColumns(updatedColumns);
    
      // Находим новую позицию для текущей колонки
      const newPos = updatedColumns.find(col => col.id === active.id)?.position;
    
      console.log("Old DND index:", oldIndex, "Old pos:", columns[oldIndex].position, "NewPos:", newPos, "New DND index:", newIndex);
      
      fetchData(`/columnMove?ColumnId=${columns[oldIndex].id.slice(0,-1)}&OldPosition=${columns[oldIndex].position}&NewPosition=${newPos}&SpaceId=${TEST_DATA__SPACE_ID}&BoardId=${TEST_DATA__BOARD_ID}`)
        .then(response => console.log(response));
    
      console.log("Columns reordered:", updatedColumns);
    }
    
    
    // Перетаскивание задач
    else if (active.data.current?.type === "task") {
      const activeColumnId = active.data.current?.columnId;
      const overColumnId = over.data.current?.columnId;

      if (!activeColumnId || !overColumnId) {
        console.log("Missing column IDs");
        setActiveTask(null);
        setActiveColumn(null);
        return;
      }

      const activeColumn = columns.find((col) => col.id === activeColumnId);
      const overColumn = columns.find((col) => col.id === overColumnId);

      if (!activeColumn || !overColumn) {
        console.log("Columns not found");
        setActiveTask(null);
        setActiveColumn(null);
        return;
      }

      const activeTaskIndex = activeColumn.tasks.findIndex(
        (task) => task.id === active.id
      );
      const overTaskIndex = over.data.current?.taskIndex;

      if (activeColumnId === overColumnId) {
        if (overTaskIndex !== undefined && activeTaskIndex !== overTaskIndex) {
          const reorderedTasks = arrayMove(
            activeColumn.tasks,
            activeTaskIndex,
            overTaskIndex
          );
          setColumns(
            columns.map((col) =>
              col.id === activeColumnId ? { ...col, tasks: reorderedTasks } : col
            )
          );
        }
      } else {
        const task = activeColumn.tasks[activeTaskIndex];
        const updatedActiveTasks = activeColumn.tasks.filter(
          (t) => t.id !== active.id
        );
        const updatedOverTasks = [...overColumn.tasks];
        updatedOverTasks.splice(overTaskIndex || 0, 0, task);

        setColumns(
          columns.map((col) => {
            if (col.id === activeColumnId)
              return { ...col, tasks: updatedActiveTasks };
            if (col.id === overColumnId)
              return { ...col, tasks: updatedOverTasks };
            return col;
          })
        );
      }
    }

    setActiveTask(null);
    setActiveColumn(null);
  };

  return (

    <>
    
    <div className="main">
      <div className="topbar">
        <div className="topbar-logo">
          <h1>logo</h1>
        </div>
        <div className="topbar-content">
          table
        </div>
      </div>

      <div className="flex-column">
        <Sidebar />
        <div className="app">

        <div className="scroll-wrapper" ref={scrollRef}>
        <div className="columns-container">
          <DndContext onDragStart={handleDragStart} onDragEnd={handleDragEnd}>
            <SortableContext items={columns.map((col) => col.id)}>
              {columns.map((col) => (
                <Columns
                  key={col.id}
                  colId={col.id}
                  colName={col.title}
                  tasks={col.tasks}
                  setColumns={setColumns}
                />
              ))}
            </SortableContext>

            <DragOverlay>
              {activeTask ? (
                <TaskItem
                task={activeTask}
                columnId={activeTask.columnId}
                  taskIndex={0}
                />
              ) : activeColumn ? (
                <Columns
                colId={activeColumn.id}
                  colName={activeColumn.title}
                  tasks={activeColumn.tasks}
                  setColumns={setColumns}
                />
              ) : null}
            </DragOverlay>
          </DndContext>
          <button type="button" className="app_addButton hover_darkgray" onClick={handleAddColumn}>+</button>
        </div>
      </div>

        </div>
      </div>
    
    
    </div>
    


    
              </>
  );
}

export default App;