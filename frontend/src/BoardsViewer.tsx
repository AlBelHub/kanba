import { useEffect, useState } from "react";
import { Link, useLoaderData, useParams } from "react-router";
import { useAppStore } from "./Store/useAppStore";
import { Board } from "./Types/types";
import { createBoard, getBoards } from "./Utils/Api";

export default function BoardsViewer() {
  const { spaceId } = useParams();
  const setSpaceId = useAppStore((s) => s.setSpaceId);


  const userId = useAppStore.getState().userId;
  
  useEffect(() => {
      //CHECK IF UUID IS VALID
      if (spaceId === "undefined") {
          throw new Error("spaceId URL error");
        }
        
        setSpaceId(String(spaceId));

    useAppStore.getState().setBoard(null);
    getBoards(String(spaceId)).then((data) => setBoards(data));
  }, [userId, spaceId]);

  const [boards, setBoards] = useState<Board[]>([
      {
      id: "CREATED_MANUALLY_1",
      name: "My Board1",
      space_id: String(spaceId),
      owner_id: String(userId),
    }
  ]);


  const handleAddBoard = async () => {

    console.log(`USER IS ${userId}`)
    console.log(`SPACE ID IS ${spaceId}`)

    const newBoard: Board = await createBoard({
      name: "Yay! New board!",
      owner_id: String(userId),
      space_id: String(spaceId),
    });
    setBoards([...boards, newBoard]);
  };


  return (
    <>
      <div className="nothing">
        <button
          type="button"
          className="sidebar-button hover_darkgray"
          onClick={handleAddBoard}
        >
          Добавить доску
        </button>
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
