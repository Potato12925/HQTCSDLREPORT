import axios from "axios";

const api = axios.create({
  baseURL: "/api",
  withCredentials: true,
});

export const testApi = ({ server, database }) => {
  return axios.post("/api/Data/test", {
    server,
    database,
  });
};

export const getDatabasesApi = (server) => {
  return api.get("/Data/databases", {
    params: { server },
  });
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
