' A Module is VB's version of a static class: every member is Shared,
' and C# calls it like ReferralRules.Urgency(...).
Public Module ReferralRules

    ''' <summary>True when the due date is before today.</summary>
    Public Function IsOverdue(dueDate As Date, today As Date) As Boolean
        Return dueDate.Date < today.Date
    End Function

    Public Function DaysUntilDue(dueDate As Date, today As Date) As Integer
        Return (dueDate.Date - today.Date).Days
    End Function

    ''' <summary>Classifies a referral as overdue, due_soon (0-2 days), or on_track.</summary>
    Public Function Urgency(dueDate As Date, today As Date) As String
        Dim days = DaysUntilDue(dueDate, today)

        Select Case days
            Case Is < 0
                Return "overdue"
            Case 0 To 2
                Return "due_soon"
            Case Else
                Return "on_track"
        End Select
    End Function

End Module