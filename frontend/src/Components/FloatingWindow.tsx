import React, { useState } from "react";
import { motion, useDragControls } from "framer-motion";
import { Task } from "../Types/types";

interface FloatingWindowProps {
  task: Task;
  columnId: string;
  onClose: () => void;
  setTopWindow: (id: string) => void;
  isTop: boolean;
}

export default function FloatingWindow({ task, columnId, onClose, setTopWindow, isTop }: FloatingWindowProps) {
  const [pos, setPos] = useState({ x: 100, y: 100 });
  const [size, setSize] = useState({ width: 400, height: 250 }); 
  const dragControls = useDragControls();
  const [isResizing, setIsResizing] = useState(false);

  const handleMouseMove = (e: MouseEvent) => {
    if (isResizing) {
      setSize((prev) => ({
        width: Math.max(200, prev.width + e.movementX),
        height: Math.max(150, prev.height + e.movementY),
      }));
    }
  };

  const handleMouseUp = () => setIsResizing(false);

  return (
    <motion.div
      drag
      dragControls={dragControls}
      dragMomentum={false}
      dragListener={false}
      onDragEnd={(_, info) => setPos({ x: info.delta.x, y: info.delta.y })}
      onMouseDown={() => setTopWindow(columnId)} // Поднять окно наверх при клике
      style={{
        position: "absolute",
        left: pos.x,
        top: pos.y,
        width: size.width,
        height: size.height,
        zIndex: isTop ? 1000 : 1, // Поднимаем наверх, если isTop === true
        touchAction: "none",
      }}
    >
      <div className="window" style={{ width: size.width, height: size.height, position: "relative", userSelect: "none" }}
      >
        <div className="title-bar"
          onPointerDown={(e) => {
            e.stopPropagation();
            dragControls.start(e);
          }}
          style={{ cursor: "pointer" }}
        >
          <div className="title-bar-text">{task.title}</div>
          <div className="title-bar-controls">
            <button aria-label="Minimize" />
            <button aria-label="Maximize" />
            <button aria-label="Close" onClick={onClose} />
          </div>
        </div>
        <div className="window-body body-settings">
          Здесь может и будет находиться информация о задаче.
        </div>

        {/* Уголок для ресайза */}
        <div
          style={{
            position: "absolute",
            bottom: 0,
            right: 0,
            width: "16px",
            height: "16px",
            cursor: "nwse-resize",
            background: "gray",
          }}
          onMouseDown={(e) => {
            e.preventDefault();
            setIsResizing(true);
          }}
        />
      </div>

      {/* Глобальный слушатель для ресайза */}
      {isResizing && (
        <div
          style={{
            position: "fixed",
            top: 0,
            left: 0,
            width: "100vw",
            height: "100vh",
            zIndex: 9999,
            cursor: "nwse-resize",
          }}
          onMouseMove={handleMouseMove}
          onMouseUp={handleMouseUp}
        />
      )}
    </motion.div>
  );
}
