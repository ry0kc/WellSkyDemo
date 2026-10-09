Imports System.Threading
Imports MediatR

' What the handler needs from storage. C# implements this with EF Core.
Public Class ReferralData
    Public Property PatientId As String = ""
    Public Property Provider As String = ""
    Public Property NextStep As String = ""
    Public Property DueDate As Date?
End Class

Public Interface IReferralReader
    Function GetByPatientIdAsync(patientId As String, ct As CancellationToken) As Task(Of ReferralData)
End Interface

' What the API returns.
Public Class ReferralSummary
    Public Property PatientId As String = ""
    Public Property Provider As String = ""
    Public Property NextStep As String = ""
    Public Property DueDate As Date?
    Public Property DaysUntilDue As Integer?
    Public Property Urgency As String = ""
End Class

Public Class GetReferralSummaryQuery
    Implements IRequest(Of ReferralSummary)

    Public Property PatientId As String = ""
End Class