import React, { useEffect, useState } from "react";
import { Link, Outlet, useLoaderData, useParams } from "react-router";
import { useAppStore } from "./Store/useAppStore";
import { Board } from "./Types/types";
import { getBoards } from "./Utils/Api";

export default function BoardsViewer() {
  const [boards, setBoards] = useState(null);

  console.log(boards);

  const { userId } = useLoaderData();
  const { spaceId } = useParams();

  const setUserId = useAppStore((s) => s.setUserId);
  const setSpaceId = useAppStore((s) => s.setSpaceId);

  useEffect(() => {
    setUserId(userId);

    //CHECK IF UUID IS VALID
    if (spaceId === "undefined") {
      throw new Error("spaceId URL error");
    }

    setSpaceId(String(spaceId));

    getBoards(String(spaceId)).then((data) => setBoards(data));

    useAppStore.getState().setBoard(null)

  }, [userId, spaceId]);

  return (
    <>
      <div className="nothing">
        {boards ? (
          boards.map((board: Board) => (
            <button key={board.id} className="board">
              <Link to={`/${spaceId}/${board.id}`}>{board.name}</Link>
            </button>
          ))
        ) : (
          <p>Loading boards...</p>
        )}
      </div>
    </>
  );
}
