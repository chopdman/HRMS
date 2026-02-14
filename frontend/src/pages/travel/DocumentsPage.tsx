import { useEffect, useMemo, useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { useSearchParams } from "react-router-dom";
import { Card } from "../../components/ui/Card";
import { EmptyState } from "../../components/ui/EmptyState";
import {
  AsyncSearchableSelect,
  SearchableSelect,
} from "../../components/ui/Combobox";
import { Input } from "../../components/ui/Input";
import { Header } from "../../components/Header";
import { Spinner } from "../../components/ui/Spinner";
import { useAuth } from "../../hooks/useAuth";
import { useEmployeeSearch } from "../../hooks/useEmployeeSearch";
import { useTeamMembers } from "../../hooks/travel/useManager";
import {
  useDeleteTravelDocument,
  useUpdateTravelDocument,
  useUploadTravelDocument,
} from "../../hooks/travel/useTravelDocumentMutations";
import { useAssignedTravels } from "../../hooks/travel/useTravel";
import {
  useTravelDocuments,
  type TravelDocumentFilters,
} from "../../hooks/travel/useTravelDocuments";
import { formatDate } from "../../utils/format";
import { Button } from "../../components/ui/Button";

type DocumentFormValues = {
  travelId: number;
  employeeId?: number;
  documentType: string;
  file: FileList;
};

export const DocumentsPage = () => {
  const { role, userId } = useAuth();
  const canUpload = role === "HR" || role === "Employee";
  const isHr = role === "HR";
  const isManager = role === "Manager";
  const [message, setMessage] = useState("");
  const [searchQuery, setSearchQuery] = useState("");
  const [searchParams] = useSearchParams();
  const [editDocs, setEditDocs] = useState<
    Record<number, { documentType: string; file: File | null }>
  >({});
  const employeesQuery = useEmployeeSearch(
    searchQuery,
    isHr && searchQuery.length >= 2,
  );
  const teamMembersQuery = isManager
    ? useTeamMembers()
    : { isLoading: false, data: [] };
  const {
    watch: watchFilters,
    control: controlFilters,
    setValue: setFilterValue,
  } = useForm<TravelDocumentFilters>({
    defaultValues: {
      travelId: undefined,
      employeeId: undefined,
    },
  });

  const {
    register: registerForm,
    handleSubmit,
    watch,
    setValue,
    reset,
    formState: { errors },
  } = useForm<DocumentFormValues>({
    defaultValues: {
      travelId: undefined,
      employeeId: undefined,
      documentType: "",
    },
  });

  const filters = watchFilters();
  const selectedEmployeeId = filters.employeeId
    ? Number(filters.employeeId)
    : undefined;
  const canFetchTravels = role === "Employee" || Boolean(selectedEmployeeId);
  const travelOptionsQuery = useAssignedTravels(
    selectedEmployeeId,
    canFetchTravels,
  );
  const normalizedFilters = useMemo(
    () => ({
      travelId: filters.travelId ? Number(filters.travelId) : undefined,
      employeeId: filters.employeeId ? Number(filters.employeeId) : undefined,
    }),
    [filters],
  );

  const { data, isLoading, isError, refetch } =
    useTravelDocuments(normalizedFilters);
  const uploadMutation = useUploadTravelDocument();
  const updateMutation = useUpdateTravelDocument();
  const deleteMutation = useDeleteTravelDocument();

  const employeeOptions = useMemo(() => {
    if (isHr) {
      return employeesQuery.data ?? [];
    }

    if (isManager) {
      return (teamMembersQuery.data ?? []).map((member: any) => ({
        id: member.id,
        fullName: member.fullName,
        email: member.email,
      }));
    }

    return [];
  }, [employeesQuery.data, isHr, isManager, teamMembersQuery.data]);

  const listLoading = isHr
    ? employeesQuery.isLoading
    : teamMembersQuery.isLoading;

  const uploadEmployeeId = watch("employeeId")
    ? Number(watch("employeeId"))
    : undefined;
  const resolvedUploadEmployeeId = isHr ? uploadEmployeeId : userId;
  const canLoadUploadTravels = Boolean(resolvedUploadEmployeeId);
  const uploadTravelsQuery = useAssignedTravels(
    resolvedUploadEmployeeId,
    canLoadUploadTravels,
  );

  useEffect(() => {
    const travelParam = searchParams.get("travelId");
    const employeeParam = searchParams.get("employeeId");

    if (travelParam) {
      setFilterValue("travelId", Number(travelParam));
    }

    if (employeeParam) {
      setFilterValue("employeeId", Number(employeeParam));
    }
  }, [searchParams, setFilterValue]);

  const onUpload = async (values: DocumentFormValues) => {
    setMessage("");
    const file = values.file?.item(0);
    if (!file) {
      return;
    }

    await uploadMutation.mutateAsync({
      travelId: Number(values.travelId),
      employeeId: values.employeeId ? Number(values.employeeId) : undefined,
      documentType: values.documentType,
      file,
    });

    reset();
    await refetch();
    setMessage("Document uploaded successfully.");
  };

  const handleEditChange = (
    documentId: number,
    field: "documentType" | "file",
    value: string | File | null,
  ) => {
    setEditDocs((prev) => ({
      ...prev,
      [documentId]: {
        documentType:
          field === "documentType"
            ? String(value)
            : (prev[documentId]?.documentType ?? ""),
        file:
          field === "file"
            ? (value as File | null)
            : (prev[documentId]?.file ?? null),
      },
    }));
  };

  const handleSaveEdit = async (documentId: number) => {
    const payload = editDocs[documentId];
    if (!payload) {
      return;
    }

    await updateMutation.mutateAsync({
      documentId,
      documentType: payload.documentType || undefined,
      file: payload.file ?? undefined,
    });

    setEditDocs((prev) => {
      const next = { ...prev };
      delete next[documentId];
      return next;
    });
    await refetch();
  };

  const handleDeleteDoc = async (documentId: number) => {
    await deleteMutation.mutateAsync(documentId);
    await refetch();
  };

  return (
    <section className="space-y-6">
      <Header
        title="Travel documents"
        description="Browse uploaded travel documents and filter by travel or employee."
      />

      {canUpload ? (
        <Card className="space-y-4">
          <div>
            <h3 className="text-base font-semibold text-slate-900">
              Upload document
            </h3>
            <p className="text-xs text-slate-500">
              Provide assignment and travel details to upload.
            </p>
          </div>
          <form
            className="grid gap-4 md:grid-cols-2"
            onSubmit={handleSubmit(onUpload)}
          >
            <input
              type="hidden"
              {...registerForm("travelId", {
                required: "Travel is required.",
                valueAsNumber: true,
              })}
            />
            {isHr ? (
              <AsyncSearchableSelect
                label="Employee (optional)"
                error={errors.employeeId?.message}
                options={employeeOptions.map((employee: any) => ({
                  value: employee.id,
                  label: `${employee.fullName} (${employee.email})`,
                }))}
                value={watch("employeeId")}
                onChange={(value) => {
                  setValue("employeeId", value);
                  setValue("travelId", undefined!);
                }}
                onSearch={setSearchQuery}
                isLoading={listLoading}
              />
            ) : null}
            {resolvedUploadEmployeeId ? (
              <SearchableSelect
                label="Travel"
                options={
                  uploadTravelsQuery.data?.map((travel: any) => ({
                    value: travel.travelId,
                    label: `${travel.travelName} (${formatDate(travel.startDate)} → ${formatDate(travel.endDate)})`,
                  })) ?? []
                }
                value={watch("travelId")}
                onChange={(value) =>
                  setValue("travelId", value as number, {
                    shouldValidate: true,
                  })
                }
                error={errors.travelId?.message}
                disabled={uploadTravelsQuery.isLoading}
              />
            ) : (
              <Input
                label="Travel ID"
                type="number"
                error={errors.travelId?.message}
                {...registerForm("travelId", {
                  required: "Travel ID is required.",
                  valueAsNumber: true,
                })}
              />
            )}
            <Input
              label="Document type"
              error={errors.documentType?.message}
              {...registerForm("documentType", {
                required: "Document type is required.",
              })}
            />
            <Input
              label="File"
              type="file"
              error={errors.file?.message}
              {...registerForm("file", { required: "File is required." })}
            />
            <div className="md:col-span-2">
              <Button
                className="inline-flex w-full items-center justify-center rounded-md bg-brand-600 px-4 py-2 text-sm font-semibold text-black hover:bg-brand-700 disabled:opacity-70"
                type="submit"
                disabled={uploadMutation.isPending}
              >
                {uploadMutation.isPending ? "Uploading..." : "Upload document"}
              </Button>
            </div>
          </form>
          {message ? (
            <p className="text-sm text-emerald-600">{message}</p>
          ) : null}
        </Card>
      ) : null}

      <Card className="space-y-4">
        <div>
          <h3 className="text-base font-semibold text-slate-900">
            Filter documents
          </h3>
          <p className="text-xs text-slate-500">
            Refine results using travel or employee identifiers.
          </p>
        </div>
        <div className="grid gap-4 md:grid-cols-2">
          <Controller
            name="employeeId"
            control={controlFilters}
            render={({ field }) => (
              <AsyncSearchableSelect
                label="Employee"
                options={employeeOptions.map((employee: any) => ({
                  value: employee.id,
                  label: `${employee.fullName} (${employee.email})`,
                }))}
                value={field.value}
                onChange={(value) => {
                  field.onChange(value);
                  setFilterValue("travelId", undefined);
                }}
                onSearch={setSearchQuery}
                isLoading={listLoading}
              />
            )}
          />
          <SearchableSelect
            label="Travel"
            options={
              travelOptionsQuery.data?.map((travel: any) => ({
                value: travel.travelId,
                label: `${travel.travelName} (${formatDate(travel.startDate)} → ${formatDate(travel.endDate)})`,
              })) ?? []
            }
            value={filters.travelId}
            onChange={(value) => setFilterValue("travelId", value)}
            error={undefined}
          />
        </div>
      </Card>
      {isLoading ? (
        <div className="flex items-center gap-2 text-sm text-slate-500">
          <Spinner /> Loading documents...
        </div>
      ) : null}

      {isError ? (
        <Card>
          <p className="text-sm text-red-600">
            Unable to load documents right now.
          </p>
        </Card>
      ) : null}

      {data?.length ? (
        <div className="space-y-3">
          {data.map((doc: any) => (
            <Card
              key={doc.documentId}
              className="flex flex-col gap-2 md:flex-row md:items-center md:justify-between"
            >
              <div>
                <p className="text-sm font-semibold text-slate-900">
                  {doc.documentType}
                </p>
                <p className="text-xs text-slate-500">{doc.fileName}</p>
                <p className="text-xs text-slate-500">
                  Uploaded {formatDate(doc.uploadedAt)}
                </p>
                {doc.employeeId ? (
                  <p className="text-xs text-slate-500">
                    Employee ID: {doc.employeeId}
                  </p>
                ) : (
                  <p className="text-xs text-slate-500">
                    Applies to all assigned employees
                  </p>
                )}
                {editDocs[doc.documentId] ? (
                  <div className="mt-2 grid gap-2 md:grid-cols-2">
                    <Input
                      label="Document type"
                      value={editDocs[doc.documentId].documentType}
                      onChange={(event) =>
                        handleEditChange(
                          doc.documentId,
                          "documentType",
                          event.target.value,
                        )
                      }
                    />
                    <Input
                      label="Replace file"
                      type="file"
                      onChange={(event) =>
                        handleEditChange(
                          doc.documentId,
                          "file",
                          event.target.files?.item(0) ?? null,
                        )
                      }
                    />
                  </div>
                ) : null}
              </div>
              <div className="flex flex-wrap items-center gap-2">
                <a
                  className="text-sm font-semibold text-brand-600 hover:text-brand-700"
                  href={doc.filePath}
                  target="_blank"
                  rel="noreferrer"
                >
                  View file
                </a>
                {doc.uploadedById === userId ? (
                  <>
                  
                 { editDocs[doc.documentId] ? (
                    <>
                      <button
                        className="text-sm font-semibold text-emerald-600 hover:text-emerald-700"
                        type="button"
                        onClick={() => handleSaveEdit(doc.documentId)}
                        disabled={updateMutation.isPending}
                      >
                        {updateMutation.isPending ? "Saving..." : "Save"}
                      </button>
                      <button
                        className="text-sm font-semibold text-slate-500 hover:text-slate-600"
                        type="button"
                        onClick={() =>
                          setEditDocs((prev) => {
                            const next = { ...prev };
                            delete next[doc.documentId];
                            return next;
                          })
                        }
                      >
                        Cancel
                      </button>
                    </>
                  ) : (
                    <>
                      <button
                        className="text-sm font-semibold text-slate-600 hover:text-slate-700"
                        type="button"
                        onClick={() =>
                          setEditDocs((prev) => ({
                            ...prev,
                            [doc.documentId]: {
                              documentType: doc.documentType ?? "",
                              file: null,
                            },
                          }))
                        }
                      >
                        Edit
                      </button>
                      <button
                        className="text-sm font-semibold text-red-600 hover:text-red-700"
                        type="button"
                        onClick={() => handleDeleteDoc(doc.documentId)}
                        disabled={deleteMutation.isPending}
                      >
                        Delete
                      </button>
                    </>
                  )
                 } </>): null}
              </div>
            </Card>
          ))}
        </div>
      ) : null}

      {!isLoading && !isError && !data?.length ? (
        <EmptyState
          title="No documents yet"
          description="Upload travel documents to see them listed here."
        />
      ) : null}
    </section>
  );
};
