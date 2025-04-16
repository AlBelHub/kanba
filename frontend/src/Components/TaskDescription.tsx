import { ChangeEvent, useEffect, useState } from "react";
import { getTaskDetail } from "../Utils/Api";
import { TaskWithUsersDto } from "../Types/types";

export default function TaskDescription({ id }) {
  const [details, setDetails] = useState<TaskWithUsersDto | null>(null);
  const [editing, setEditing] = useState<boolean>(false);

  useEffect(() => {
    getTaskDetail(id).then((data) => setDetails(data));
  }, [id]);

  const handleDescrEditButton = () => setEditing(!editing);

  const handleDescrEditButtonOK = () => {
    setEditing(!editing);
  };

  const inputDescriptionValueChange = (e: ChangeEvent<HTMLInputElement>) => {
    if (details) {
      setDetails({
        ...details,
        description: e.target.value,
      });
    }
  };

  return (
    <>
      <div>
        Автор: {details?.created_by_username}
        <button>изм.</button>
      </div>
      <div>
        Описание:{" "}
        {editing ? (
          <input
            onChange={inputDescriptionValueChange}
            name="descr"
            defaultValue={`${details?.description}`}
          />
        ) : (
          details?.description
        )}
        {!editing ? (
          <button onClick={handleDescrEditButton}>изм.</button>
        ) : (
          <button onClick={handleDescrEditButtonOK}>OK</button>
        )}
      </div>
      <div>
        Статус: {details?.status}
        <button>изм.</button>
      </div>
    </>
  );
}
