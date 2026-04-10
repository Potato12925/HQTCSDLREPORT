import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/Home.vue'
import SqlExecuteView from '../views/SqlExecuteView.vue'
import SqlReportView from '../views/SqlReportView.vue'

const routes = [
  { path: '/', name: 'Home', component: Home },
  { path: '/sql-execute', name: 'SqlExecute', component: SqlExecuteView },
  { path: '/sql-report', name: 'SqlReport', component: SqlReportView },
]

const router = createRouter({
  history: createWebHistory(), // thay cho mode: 'history'
  routes
})

export default router
