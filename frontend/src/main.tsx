import { createRoot } from "react-dom/client";
import App from "./App.tsx";
import RootLayout from "./RootLayout.tsx";

import { createBrowserRouter, RouterProvider } from "react-router";
import BoardsViewer from "./BoardsViewer.tsx";
import LoginPage from "./Components/LoginPage.tsx";

const router = createBrowserRouter([
  {
    path: "/",
    element: <RootLayout />,
    children: [
      {
        index: true,
        element: <LoginPage />
      },
      {
        path: "/:spaceId",
        element: <BoardsViewer />,
      },
      {
        path: "/:spaceId/:boardId",
        element: <App />, 
      },
    ]    
  }
])

createRoot(document.getElementById("root")!).render(
  <RouterProvider router={router} />
);
