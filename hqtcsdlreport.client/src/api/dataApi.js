import axios from "axios";

const api = axios.create({
  baseURL: "/api",
  withCredentials: true,
});

export const testApi = () => {
  return api.get("/Data/test");
};

export const getDatabasesApi = (server) => {
  return api.get("/Data/databases", {
    params: { server },
  });
};

export const queryApi = () => {
  return api.get("/Data/query");
};

export const connectDbApi = (data) => {
  return api.post("/Data/connect", data);
};

export const executeSqlApi = (data) => {
  return api.post("/Data/execute", data);
};

export const prepareReportApi = (data) => {
  return api.post("/Report/prepare", data);
};
