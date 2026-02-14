import { useMemo, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { createSearchParams, useNavigate } from "react-router-dom";
import {
  AsyncSearchableMultiSelect,
  AsyncSearchableSelect,
} from "../../components/ui/Combobox";
import { Card } from "../../components/ui/Card";
import { EmptyState } from "../../components/ui/EmptyState";
import { Input } from "../../components/ui/Input";
import { Header } from "../../components/Header";
import { Spinner } from "../../components/ui/Spinner";
import { useAuth } from "../../hooks/useAuth";
import { useEmployeeSearch } from "../../hooks/useEmployeeSearch";
import { useTeamMembers } from "../../hooks/travel/useManager";
import { useAssignedTravels } from "../../hooks/travel/useTravel";
import {
  useCreateTravel,
  useDeleteTravel,
  useUpdateTravel,
} from "../../hooks/travel/useTravelMutations";
import { formatDate } from "../../utils/format";
import { Badge } from "../../components/ui/Badge";
import { Button } from "../../components/ui/Button";

type TravelCreateFormValues = {
  travelName: string;
  destination: string;
  purpose?: string;
  startDate: string;
  endDate: string;
  assignments: number[];
};

type TravelFilterValues = {
  employeeId?: number;
};

type TravelEditFormValues = {
  travelName: string;
  destination: string;
  purpose?: string;
  startDate: string;
  endDate: string;
};

export const TravelsPage = () => {
  const { role, userId } = useAuth();
  const isEmployee = role === "Employee";
  const isHr = role === "HR";
  const isManager = role === "Manager";
  const [message, setMessage] = useState("");
  const [searchQuery, setSearchQuery] = useState("");
  const [editingTravelId, setEditingTravelId] = useState<number | null>(null);
  const [editMessage, setEditMessage] = useState("");
  const navigate = useNavigate();

  const { watch, control: controlFilter } = useForm<TravelFilterValues>({
    defaultValues: { employeeId: undefined },
  });

  const selectedEmployeeId = watch("employeeId");
  const normalizedEmployeeId = selectedEmployeeId
    ? Number(selectedEmployeeId)
    : undefined;
  const travelEmployeeId = isEmployee ? userId : normalizedEmployeeId;
  const canFetch = isEmployee ? Boolean(userId) : Boolean(normalizedEmployeeId);

  const { data, isLoading, isError, refetch } = useAssignedTravels(
    travelEmployeeId,
    canFetch,
  );
  const createTravel = useCreateTravel();
  const updateTravel = useUpdateTravel();
  const deleteTravel = useDeleteTravel();
  const employeesQuery = useEmployeeSearch(
    searchQuery,
    isHr && searchQuery.length >= 2,
  );
  const teamMembersQuery = isManager
    ? useTeamMembers()
    : { isLoading: false, data: [] };

  const listLoading = isHr
    ? employeesQuery.isLoading
    : teamMembersQuery.isLoading;

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors },
  } = useForm<TravelCreateFormValues>({
    defaultValues: {
      travelName: "",
      destination: "",
      purpose: "",
      startDate: "",
      endDate: "",
      assignments: [],
    },
  });

  const {
    register: registerEdit,
    handleSubmit: handleSubmitEdit,
    setValue: setEditValue,
    reset: resetEdit,
  } = useForm<TravelEditFormValues>({
    defaultValues: {
      travelName: "",
      destination: "",
      purpose: "",
      startDate: "",
      endDate: "",
    },
  });

  const employeeOptions = useMemo(() => {
    if (isHr) {
      return employeesQuery.data ?? [];
    }

    if (isManager) {
      return (teamMembersQuery.data ?? []).map((member) => ({
        id: member.id,
        fullName: member.fullName,
        email: member.email,
      }));
    }

    return [];
  }, [employeesQuery.data, isHr, isManager, teamMembersQuery.data]);

  const onCreateTravel = async (values: TravelCreateFormValues) => {
    setMessage("");
    if (!userId) {
      setMessage("Unable to identify the current user.");
      return;
    }

    if (!values.assignments?.length) {
      setMessage("Enter at least one employee ID to assign.");
      return;
    }

    await createTravel.mutateAsync({
      travelName: values.travelName,
      destination: values.destination,
      purpose: values.purpose || undefined,
      startDate: values.startDate,
      endDate: values.endDate,
      createdById: userId,
      assignments: values.assignments.map((employeeId) => ({ employeeId })),
    });

    reset();
    setMessage("Travel created successfully.");
  };

  const startEdit = (travelId: number, values: TravelEditFormValues) => {
    setEditMessage("");
    setEditingTravelId(travelId);
    setEditValue("travelName", values.travelName);
    setEditValue("destination", values.destination);
    setEditValue("purpose", values.purpose ?? "");
    setEditValue("startDate", values.startDate);
    setEditValue("endDate", values.endDate);
  };

  const onUpdateTravel = async (values: TravelEditFormValues) => {
    if (!editingTravelId) {
      return;
    }

    await updateTravel.mutateAsync({
      travelId: editingTravelId,
      travelName: values.travelName,
      destination: values.destination,
      purpose: values.purpose || undefined,
      startDate: values.startDate,
      endDate: values.endDate,
    });

    setEditingTravelId(null);
    resetEdit();
    await refetch();
    setEditMessage("Travel updated successfully.");
  };

  const onDeleteTravel = async (travelId: number) => {
    if (!globalThis.confirm("Delete this travel plan? This cannot be undone.")) {
      return;
    }

    await deleteTravel.mutateAsync(travelId);
    await refetch();
  };

  const goToDocuments = (travelId: number) => {
    const params: Record<string, string> = { travelId: String(travelId) };
    if (!isEmployee && normalizedEmployeeId) {
      params.employeeId = String(normalizedEmployeeId);
    }
    navigate({
      pathname: "/documents",
      search: createSearchParams(params).toString(),
    });
  };

  return (
    <section className="space-y-6">
      <Header title="Travels" />

      {isHr ? (
        <Card className="space-y-4">
          <div>
            <h3 className="text-base font-semibold text-slate-900">
              Create travel plan
            </h3>
            <p className="text-xs text-slate-500">
              Assign employees using the dropdown list.
            </p>
          </div>
          <form
            className="grid gap-4 md:grid-cols-2"
            onSubmit={handleSubmit(onCreateTravel)}
          >
            <Input
              label="Travel name"
              error={errors.travelName?.message}
              {...register("travelName", {
                required: "Travel name is required.",
              })}
            />
            <Input
              label="Destination"
              error={errors.destination?.message}
              {...register("destination", {
                required: "Destination is required.",
              })}
            />
            <Input label="Purpose" {...register("purpose")} />
            <Controller
              name="assignments"
              control={control}
              rules={{
                validate: (value) =>
                  value && value.length > 0
                    ? true
                    : "At least one employee must be assigned.",
              }}
              render={({ field }) => (
                <AsyncSearchableMultiSelect
                  label="Assignments"
                  options={
                    Array.isArray(employeeOptions)
                      ? employeeOptions.map((employee) => ({
                          value: employee.id,
                          label: `${employee.fullName} (${employee.email})`,
                        }))
                      : []
                  }
                  value={field.value ?? []}
                  onChange={field.onChange}
                  onSearch={setSearchQuery}
                  isLoading={listLoading}
                  error={errors.assignments?.message}
                />
              )}
            />
            <Input
              label="Start date"
              type="date"
              error={errors.startDate?.message}
              {...register("startDate", {
                required: "Start date is required.",
              })}
            />
            <Input
              label="End date"
              type="date"
              error={errors.endDate?.message}
              {...register("endDate", { required: "End date is required." })}
            />
            <div className="md:col-span-2">
              <Button
                className="inline-flex w-full items-center justify-center rounded-md bg-brand-600 px-4 py-2 text-sm font-semibold text-black hover:bg-brand-700 disabled:opacity-70"
                type="submit"
                disabled={createTravel.isPending}
              >
                {createTravel.isPending ? "Creating..." : "Create travel"}
              </Button>
            </div>
          </form>
          {message ? (
            <p className="text-sm text-emerald-600">{message}</p>
          ) : null}
        </Card>
      ) : null}

      {!isEmployee  ? (
        <Card className="space-y-4">
          <div>
            <h3 className="text-base font-semibold text-slate-900">
              Lookup assigned travels
            </h3>
            <p className="text-xs text-slate-500">
              Managers and HR must select an employee.
            </p>
          </div>
          <div className="grid gap-4 md:grid-cols-2">
            <Controller
              name="employeeId"
              control={controlFilter}
              render={({ field }) => (
                <AsyncSearchableSelect
                  label="Employee"
                  options={employeeOptions.map((employee) => ({
                    value: employee.id,
                    label: `${employee.fullName} (${employee.email})`,
                  }))}
                  value={field.value}
                  onChange={field.onChange}
                  onSearch={setSearchQuery}
                  isLoading={listLoading}
                />
              )}
            />
          </div>
        </Card>
      ) : null}

      {isLoading ? (
        <div className="flex items-center gap-2 text-sm text-slate-500">
          <Spinner /> Loading assigned travels...
        </div>
      ) : null}

      {isError ? (
        <Card>
          <p className="text-sm text-red-600">
            Unable to load travels right now.
          </p>
        </Card>
      ) : null}

      {data?.length ? (
        <div className="grid gap-4 md:grid-cols-2">
          {data.map((travel) => (
            <Card key={travel.travelId} className="space-y-3">
              <div className="flex items-start justify-between">
                <div>
                  <h3 className="text-lg font-semibold text-slate-900">
                    {travel.travelName}
                  </h3>
                  <p className="text-sm text-slate-500">{travel.destination}</p>
                </div>
                <Badge tone="info">Assigned</Badge>
              </div>
              <div className="text-sm text-slate-600">
                <span className="font-medium">Dates:</span>{" "}
                {formatDate(travel.startDate)} → {formatDate(travel.endDate)}
              </div>
              <div className="flex flex-wrap gap-2">
                <button
                  className="text-sm font-semibold text-brand-600 hover:text-brand-700"
                  type="button"
                  onClick={() => goToDocuments(travel.travelId)}
                >
                  View documents
                </button>
                {isHr ? (
                  <>
                    <button
                      className="text-sm font-semibold text-slate-600 hover:text-slate-700"
                      type="button"
                      onClick={() =>
                        startEdit(travel.travelId, {
                          travelName: travel.travelName,
                          destination: travel.destination,
                          purpose: "",
                          startDate: travel.startDate,
                          endDate: travel.endDate,
                        })
                      }
                    >
                      Edit
                    </button>
                    <button
                      className="text-sm font-semibold text-red-600 hover:text-red-700"
                      type="button"
                      onClick={() => onDeleteTravel(travel.travelId)}
                    >
                      Delete
                    </button>
                  </>
                ) : null}
              </div>
              {isHr && editingTravelId === travel.travelId ? (
                <form
                  className="grid gap-3"
                  onSubmit={handleSubmitEdit(onUpdateTravel)}
                >
                  <Input
                    label="Travel name"
                    {...registerEdit("travelName", { required: true })}
                  />
                  <Input
                    label="Destination"
                    {...registerEdit("destination", { required: true })}
                  />
                  <Input label="Purpose" {...registerEdit("purpose")} />
                  <Input
                    label="Start date"
                    type="date"
                    {...registerEdit("startDate", { required: true })}
                  />
                  <Input
                    label="End date"
                    type="date"
                    {...registerEdit("endDate", { required: true })}
                  />
                  <div className="flex flex-wrap gap-2">
                    <button
                      className="inline-flex items-center justify-center rounded-md bg-brand-600 px-4 py-2 text-sm font-semibold text-black hover:bg-brand-700 disabled:opacity-70"
                      type="submit"
                      disabled={updateTravel.isPending}
                    >
                      {updateTravel.isPending ? "Saving..." : "Save changes"}
                    </button>
                    <button
                      className="inline-flex items-center justify-center rounded-md border border-slate-200 px-4 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50"
                      type="button"
                      onClick={() => setEditingTravelId(null)}
                    >
                      Cancel
                    </button>
                  </div>
                </form>
              ) : null}
            </Card>
          ))}
        </div>
      ) : null}

      {!isLoading && !isError && !data?.length ? (
        <EmptyState
          title="No travel plans yet"
          description={
            canFetch
              ? "Assigned travels will appear here once HR creates and assigns a plan."
              : "Enter an employee ID to view assigned travel plans."
          }
        />
      ) : null}
      {editMessage ? (
        <p className="text-sm text-emerald-600">{editMessage}</p>
      ) : null}
    </section>
  );
};
