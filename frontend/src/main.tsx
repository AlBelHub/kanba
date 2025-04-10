import { createRoot } from "react-dom/client";
import App from "./App.tsx";
import RootLayout from "./RootLayout.tsx";

import { createBrowserRouter, RouterProvider } from "react-router";
import { initApp } from "./TEMP_DELETE/InitApp.ts";
import BoardsViewer from "./BoardsViewer.tsx";

const router = createBrowserRouter([
  {
    path: "/",
    element: <RootLayout />,
    children: [
      {
        path: "/:spaceId",
        element: <BoardsViewer />,
        loader: initApp
      },
      {
        path: "/:spaceId/:boardId",
        element: <App />, 
        loader: initApp
      },
    ]    
  }
])

createRoot(document.getElementById("root")!).render(
  <RouterProvider router={router} />
);
