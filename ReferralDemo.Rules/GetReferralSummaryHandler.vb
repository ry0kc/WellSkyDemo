Imports System.Threading
Imports MediatR

Public Class GetReferralSummaryHandler
    Implements IRequestHandler(Of GetReferralSummaryQuery, ReferralSummary)

    Private ReadOnly _reader As IReferralReader
    Private ReadOnly _clock As TimeProvider

    Public Sub New(reader As IReferralReader, clock As TimeProvider)
        _reader = reader
        _clock = clock
    End Sub

    Public Async Function Handle(request As GetReferralSummaryQuery, cancellationToken As CancellationToken) _
        As Task(Of ReferralSummary) _
        Implements IRequestHandler(Of GetReferralSummaryQuery, ReferralSummary).Handle

        Dim referral = Await _reader.GetByPatientIdAsync(request.PatientId, cancellationToken)
        If referral Is Nothing Then Return Nothing

        Dim today = _clock.GetLocalNow().Date

        Dim summary = New ReferralSummary With {
            .PatientId = referral.PatientId,
            .Provider = referral.Provider,
            .NextStep = referral.NextStep,
            .DueDate = referral.DueDate,
            .Urgency = "no_due_date"
        }

        If referral.DueDate.HasValue Then
            Dim due = referral.DueDate.Value
            summary.DaysUntilDue = DaysUntilDue(due, today)
            summary.Urgency = Urgency(due, today)
        End If

        Return summary
    End Function

End Class