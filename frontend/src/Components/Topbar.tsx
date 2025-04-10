import React from "react";
import { useAppStore } from "../Store/useAppStore";

export default function Topbar() {

    const board = useAppStore((s) => s.currentBoard)

  return (
    <div className="topbar">
      <div className="topbar-logo">
        <h1>logo</h1>
      </div>
      <div className="topbar-content">
        {board ? board.name : "Выбери над чем работать сегодня..."}
      </div>
    </div>
  );
}
