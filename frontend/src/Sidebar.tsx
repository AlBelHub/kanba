import React, { useEffect, useState } from 'react'
import { createBoard, getBoards } from './Utils/Api';
import { Board } from './Types/types';
import { useAppStore } from './Store/useAppStore';

export default function Sidebar() {

    const { spaceId, boardId, userId } = useAppStore.getState();

    const [boards, setBoards] = useState<Board[]>(
        [
          {
            "id": "CREATED_MANUALLY_1",
            "name": "My Board1",
            "space_id": spaceId,
            "owner_id": userId,
          },
          {
            "id": "CREATED_MANUALLY_2",
            "name": "My Board2",
            "space_id": spaceId,
            "owner_id": userId,
          },
          {
            "id": "CREATED_MANUALLY_3",
            "name": "My Board3",
            "space_id": spaceId,
            "owner_id": userId,
          },
        
        ]
      )

      useEffect(() => {
  
        getBoards(spaceId).then((data) => setBoards(data))
        // fetchData(`/Boards/getBoards/${TEST_DATA__SPACE_ID}`).then(data => setBoards(data));
    
      }, []);
    
      const handleAddBoard = async () => {
        const newBoard : Board = await createBoard({name: "Yay! New board!", owner_id: userId, space_id: spaceId})
        setBoards([...boards, newBoard]);
      }

    return(
        <div className="sidebar">
        

        {/* Выделить в компонент */}
            <div className="user-container">
                <div className="user-img">Uimg</div>

                <div className="user-data-container">

                    <div className="user-data">Белов Алексей</div>
                    <div className="user-status">Занят задачей №0</div>

                </div>
            </div>


            <div className="sidebar-buttons">
              <button type="button" className='sidebar-button hover_darkgray' onClick={handleAddBoard}>Добавить доску</button>
            
                {
                    boards.map(board => (
                        <button type="button" className='sidebar-button hover_darkgray' key={board.id}>{board.name}</button>
                    ))
                }
            </div>

        </div>
    );
}