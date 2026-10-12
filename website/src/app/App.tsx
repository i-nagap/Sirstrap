import { BrowserRouter, Route, Routes } from "react-router-dom";
import { ServicesContext, createServices } from "@/app/services";
import { HomePage } from "@/ui/pages/HomePage";
import { NotFoundPage } from "@/ui/pages/NotFoundPage";

const services = createServices();

export function App() {
  return (
    <ServicesContext.Provider value={services}>
      <BrowserRouter basename={import.meta.env.BASE_URL}>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </BrowserRouter>
    </ServicesContext.Provider>
  );
}
