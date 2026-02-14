export type TravelDocument = {
  documentId: number
  travelId: number
  employeeId?: number | null
  uploadedById: number
  ownerType: string
  documentType: string
  fileName: string
  filePath: string
  uploadedAt: string
}
 