import { useEffect, useRef, useState } from "react";
import {
  DndContext,
  DragEndEvent,
  DragOverlay,
  DragStartEvent,
} from "@dnd-kit/core";
import { SortableContext, arrayMove } from "@dnd-kit/sortable";
import { Column, Task } from "./Types/types";
import "98.css";
import "./App.css";
import Columns from "./Components/Columns";
import TaskItem from "./Components/TaskItem";
import Sidebar from "./Sidebar";
import { createColumn, getBoards, getColumnsAndTasks, moveColumn, moveTask } from "./Utils/Api";
import FloatingWindow from "./Components/FloatingWindow";
import { useAppStore } from "./Store/useAppStore";
import { useLoaderData, useParams } from "react-router";

function App() {
  
  const {spaceId, boardId} = useParams();

  const userId = useAppStore().userId

  const setUserId = useAppStore((s) => s.setUserId);
  const setSpaceId = useAppStore((s) => s.setSpaceId);
  const setBoardId = useAppStore((s) => s.setBoardId);
  const setBoard = useAppStore((s) => s.setBoard);

  useEffect(() => {
    setUserId(String(userId));

    if (spaceId === "undefined") {
      throw new Error("spaceId URL error")
    }
    
    setSpaceId(String(spaceId));

    if (boardId === "undefined") {
      throw new Error("boardId URL error")
    }

    setBoardId(String(boardId));

    getBoards(String(spaceId)).then((data) => {
      const board = data.find((b) => b.id === boardId);
      if (board) {
        setBoard(board);
        console.log("BOARD IS SET!")
      } else {
        throw new Error("Board not found");
      }
    });


  }, [userId, spaceId, boardId]);

// ИСПРАВИТЬ АПИ МУВ ТАСК И СДЕЛАТЬ НОРМАЛЬНЫЙ РАУТ ДО НЕЁ

//#region UI 

  const [openWindows, setOpenWindows] = useState<
    { task: Task; columnId: string }[]
  >([]);

  const [topWindow, setTopWindow] = useState<string | null>(null);

  const handleTaskClick = (task: any, columnId: string) => {

    setOpenWindows((prev) => [...prev, { task, columnId }]);
  };

  const handleCloseWindow = (taskId: string) => {
    setOpenWindows((prev) => prev.filter((win) => win.task.id !== taskId));
  };

  const scrollRef = useRef<HTMLDivElement | null>(null);
  
  const [activeTask, setActiveTask] = useState<any>(null);
  const [activeColumn, setActiveColumn] = useState<any>(null);
  
  //#endregion
  const [columns, setColumns] = useState<Column[]>([
    {
      id: "todo",
      title: "Если вы видете эту задачу, то я не вижу БД",
      position: 1,
      tasks: [{ id: "task-1", title: "Первая задача", position: 1 }],
    },
  ]);
  
  // CHECK
  useEffect(() => {
    getColumnsAndTasks(spaceId, boardId).then((data) => setColumns(data))
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
  const handleAddColumn = async () => {

    let highestPos : number;

    if (columns.length > 0) {
      highestPos = Math.max(...columns.map((item) => item.position)) + 1;
    }else{
      highestPos = 1;
    }

    


    const newColumn = await createColumn({
      boardId: String(boardId),
      title: "created from button + back",
      createdBy: userId,
      position: highestPos,
    });
    newColumn.tasks = [];

    setColumns([...columns, newColumn]);
  };
  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;

    if (!over) {
      setActiveTask(null);
      setActiveColumn(null);
      return;
    }

    // Перетаскивание колонок
    if (active.data.current?.type === "column") {
      const oldIndex = columns.findIndex((col) => col.id === active.id);
      const newIndex = columns.findIndex((col) => col.id === over.id);

      if (oldIndex === -1 || newIndex === -1 || oldIndex === newIndex) {
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
      const newPos = updatedColumns.find(
        (col) => col.id === active.id
      )?.position;

      //check
      moveColumn({
        columnId: columns[oldIndex].id,
        oldPosition: columns[oldIndex].position,
        newPosition: Number(newPos),
        boardId: boardId,
      });
    }

    // Перетаскивание задач
    else if (active.data.current?.type === "task") {
      const activeColumnId = active.data.current?.columnId;
      const overColumnId = over.data.current?.columnId;

      if (!activeColumnId || !overColumnId) {
        setActiveTask(null);
        setActiveColumn(null);
        return;
      }

      const activeColumn = columns.find((col) => col.id === activeColumnId);
      const overColumn = columns.find((col) => col.id === overColumnId);
      if (!activeColumn || !overColumn) {
        setActiveTask(null);
        setActiveColumn(null);
        return;
      }

      const activeTaskIndex = activeColumn.tasks.findIndex(
        (task) => task.id === active.id
      );
      const overTaskIndex = over.data.current?.taskIndex;

      //Задача перемещена в той же колонке
      if (activeColumnId === overColumnId) {
        if (overTaskIndex !== undefined && activeTaskIndex !== overTaskIndex) {
          const reorderedTasks = arrayMove(
            activeColumn.tasks,
            activeTaskIndex,
            overTaskIndex
          );

          moveTask(
            {
              boardId: String(boardId),
              oldColumnId: activeColumnId,
              newColumnId: overColumnId,
              taskId: active.id.toString(),
              taskNewPos: !overTaskIndex ? 1 : overTaskIndex + 1,
              taskOldPos: activeTaskIndex + 1,
            }
          )

          setColumns(
            columns.map((col) =>
              col.id === activeColumnId
                ? { ...col, tasks: reorderedTasks }
                : col
            )
          );
        }

        //Задача перемещена в другой колонке
      } else {
        const task = activeColumn.tasks[activeTaskIndex];
        const updatedActiveTasks = activeColumn.tasks.filter(
          (t) => t.id !== active.id
        );
        const updatedOverTasks = [...overColumn.tasks];
        updatedOverTasks.splice(overTaskIndex || 0, 0, task);

        moveTask(
          {
            boardId: String(boardId),
            oldColumnId: activeColumnId,
            newColumnId: overColumnId,
            taskId: active.id.toString(),
            taskNewPos: !overTaskIndex ? 1 : overTaskIndex + 1,
            taskOldPos: activeTaskIndex + 1,
          }
        )

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
      
      
            <div className="scroll-wrapper" ref={scrollRef}>
              <div className="columns-container">
                <DndContext
                  onDragStart={handleDragStart}
                  onDragEnd={handleDragEnd}
                >
                  <SortableContext items={columns.map((col) => col.id)}>
                    {columns.map((col) => (
                      <Columns
                        key={col.id}
                        colId={col.id}
                        colName={col.title}
                        tasks={col.tasks}
                        onTaskClick={handleTaskClick} //???
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
                        onTaskClick={handleTaskClick}
                      />
                    ) : activeColumn ? (
                      <Columns
                        colId={activeColumn.id}
                        colName={activeColumn.title}
                        tasks={activeColumn.tasks}
                        setColumns={setColumns}
                        onTaskClick={handleTaskClick}
                      />
                    ) : null}
                  </DragOverlay>
                </DndContext>
                <button
                  type="button"
                  className="app_addButton hover_darkgray"
                  onClick={handleAddColumn}
                >
                  +
                </button>
              </div>
            </div>

      {openWindows.map(({ task, columnId }) => (
        <FloatingWindow
          key={task.id}
          task={task}
          columnId={columnId}
          onClose={() => handleCloseWindow(task.id)}
          setTopWindow={setTopWindow}
          isTop={topWindow === task.id}
        />
      ))}
    </>
  );
}

export default App;
