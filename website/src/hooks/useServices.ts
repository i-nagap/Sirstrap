import { useContext } from "react";
import { ServicesContext, type Services } from "@/app/services";

export function useServices(): Services {
  const services = useContext(ServicesContext);
  if (!services) throw new Error("useServices must be used inside ServicesContext");
  return services;
}
