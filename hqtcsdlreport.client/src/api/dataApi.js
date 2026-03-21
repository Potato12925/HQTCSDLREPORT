import axios from "axios";

const api = axios.create({
  baseURL: "/api", 
  withCredentials: true, 
});

export const testApi = () => {
  return api.get("/Data/test");
};

export const queryApi = () => {
  return api.get("/Data/query");
};

export const connectDbApi = (data) => {
  return api.post("/Data/connect", data);
};

export const reportApi = () => {
  return api.get("/Data/report");
};