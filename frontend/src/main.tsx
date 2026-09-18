import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './global/style.css'
import AccountsView from './views/AccountsView.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AccountsView />
  </StrictMode>,
)