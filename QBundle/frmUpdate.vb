﻿Public Class frmUpdate


    Private WithEvents tmr As New Timer
    Private Sub frmUpdate_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        If Not Q.App.SetRemoteInfo() Then
            MsgBox("There was an error getting update info. Check internet connection and try again.")
            btnUpdate.Enabled = False
            Exit Sub
        End If
        If CheckAndUpdateLW() Then
            btnUpdate.Enabled = True
        Else
            btnUpdate.Enabled = True
            btnUpdate.Text = "Close"
        End If

    End Sub

    Private Sub btnUpdate_Click(sender As Object, e As EventArgs) Handles btnUpdate.Click

        If btnUpdate.Text = "Close" Then
            Me.DialogResult = DialogResult.No
            Exit Sub
        End If

        If frmMain.Running Then
            If MsgBox("Do you want to stop the wallet?" & vbCrLf & " It must be stopped before updating the components.", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                Exit Sub
            End If
        End If
        If Q.App.ShouldUpdate(QGlobal.AppNames.Launcher) Then
            If MsgBox("Qbundle will automatically restart after update." & vbCrLf & " Do you want to continue?", MsgBoxStyle.YesNo) = MsgBoxResult.No Then
                Exit Sub
            End If
        End If
        btnUpdate.Enabled = False
        If frmMain.Running Then
            lblStatus.Text = "Waiting for wallet to stop"
            frmMain.StopWallet()
            tmr.Interval = 500
            tmr.Start()
            tmr.Enabled = True
        Else
            DoUpdate()
        End If

    End Sub

    Public Sub tmr_tick() Handles tmr.Tick
        If frmMain.Running = False Then
            tmr.Stop()
            tmr.Enabled = False
            DoUpdate()
        End If
    End Sub

    Private Function CheckAndUpdateLW() As Boolean

        Dim StrApp As String() = [Enum].GetNames(GetType(QGlobal.AppNames)) 'only used to count
        Dim L(2) As String
        Dim AnyUpdates As Boolean = False
        Lw1.Items.Clear()
        For t As Integer = 0 To UBound(StrApp)
            If Q.App.isInstalled(t) Then 'no reason to test non installed
                If Q.App.HasRepository(t) Then 'Is it available at repo?
                    L(0) = Q.App.GetAppNameFromId(t)
                    L(1) = Q.App.GetLocalVersion(t)
                    L(2) = Q.App.GetRemoteVersion(t)
                    Dim itm As New ListViewItem(L)

                    If Q.App.ShouldUpdate(t) Then
                        itm.SubItems(1).ForeColor = Color.DarkRed
                        AnyUpdates = True
                    Else
                        itm.SubItems(1).ForeColor = Color.DarkGreen
                    End If

                    itm.UseItemStyleForSubItems = False
                    Lw1.Items.Add(itm)
                End If
            End If
        Next

        Return AnyUpdates


    End Function

    Private Sub DoUpdate()

        Dim S As frmDownloadExtract
        Dim AppCount As Integer = UBound([Enum].GetNames(GetType(QGlobal.AppNames)))
        CheckAndUpdateLW()
        Dim res As DialogResult
        For t As Integer = 0 To AppCount
            If Q.App.ShouldUpdate(t) Then
                S = New frmDownloadExtract
                S.Appid = t
                S.Upgrade = True 'we download UpgradeUrl not full
                res = S.ShowDialog
                If res = DialogResult.Cancel Then
                    btnUpdate.Enabled = True
                    Exit Sub
                ElseIf res = DialogResult.Abort Then
                    MsgBox("Something went wrong. Internet connection might have been lost.", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
                    btnUpdate.Enabled = True
                    Exit Sub
                End If
                Q.App.SetUpdated(t)
                CheckAndUpdateLW()
            End If
        Next

        If Q.App.isUpdated(QGlobal.AppNames.Launcher) Then
            'we should restart now since we have updates pending on ourselfs
            Dim wdir As String = Application.StartupPath
            If Not wdir.EndsWith("\") Then wdir &= "\"
            If IO.File.Exists(wdir & "Updater.exe") Then
                Try
                    Me.DialogResult = DialogResult.Yes
                Catch ex As Exception
                    Generic.WriteDebug(ex)
                End Try
            End If
        End If
        Q.App.SetLocalInfo()
        CheckAndUpdateLW()
        frmMain.lblUpdates.Visible = False
        frmMain.lblUpdateAvail2.Visible = False
        pb1.Visible = False
        frmMain.SetWalletInfo()
        lblStatus.Text = "Update complete."
        btnUpdate.Text = "Close"
        btnUpdate.Enabled = True

    End Sub



End Class