import { WSAEACCES } from 'constants';
import React, { useEffect, useState } from 'react';
import './App.css';
import * as ws from './web-services'

function App() {
  const [projects,setProjects] = useState([]);

  useEffect(()=>{
    ws.getProjects().then(res => setProjects(res.data))
  },[]);


  return (
    <div >
      {projects.map( (p:any) => <div>{p.Title}</div>)}
    </div>
  );
}

export default App;
