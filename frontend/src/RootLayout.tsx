import React from "react";
import { Outlet } from "react-router";
import Sidebar from "./Sidebar";
import Topbar from "./Components/Topbar";

export default function RootLayout() {
  
  
    return (
    <>
      <div className="main">
       <Topbar />

        <div className="flex-column">
          <Sidebar />
            <div className="app">
                <Outlet />
            </div>
        </div>
        </div>
    </>
  );
}
