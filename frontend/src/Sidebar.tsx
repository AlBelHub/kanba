import React, { useEffect, useState } from 'react'
import fetchData, { createBoard } from './Utils/Api';
import { Board } from './Types/types';

export default function Sidebar() {

    const TEST_DATA__BOARD_ID = 1;

    const [boards, setBoards] = useState<Board[]>(
        [
          {
            "id": 1,
            "name": "My Board",
            "space_id": 1,
            "owner_id": 1
          },
          {
            "id": 2,
            "name": "Web Development",
            "space_id": 1,
            "owner_id": 1
          },
          {
            "id": 3,
            "name": "My Boasdadasard",
            "space_id": 1,
            "owner_id": 1
          },
          {
            "id": 4,
            "name": "Web Develagsgsdgagopment",
            "space_id": 1,
            "owner_id": 1
          }
        ]
      )


      useEffect(() => {
  
        fetchData(`/boards/${TEST_DATA__BOARD_ID}`).then(data => setBoards(data));
    
      }, []);
    
      const handleAddBoard = async () => {
        const newBoard : Board = await createBoard({name: "Yay! New board!", owner_id: 1, space_id: 1})
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