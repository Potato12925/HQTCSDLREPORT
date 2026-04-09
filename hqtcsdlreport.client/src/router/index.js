import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/Home.vue'
import SqlExecuteView from '../views/SqlExecuteView.vue'

const routes = [
  { path: '/', name: 'Home', component: Home },
  { path: '/sql-execute', name: 'SqlExecute', component: SqlExecuteView },
]

const router = createRouter({
  history: createWebHistory(), // thay cho mode: 'history'
  routes
})

export default router
