Public Class Form1
    Dim WithEvents Time As New Timer
    Dim g As BufferedGraphics
    Dim b As BufferedGraphicsContext
    Dim Rnd As New Random
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.b = BufferedGraphicsManager.Current
        Me.g = Me.b.Allocate(Me.CreateGraphics, Me.DisplayRectangle)
        Init()
        Time.Interval = Int(1000 / 60)
        Time.Start()
    End Sub
    Private Sub Time_Tick(sender As Object, e As EventArgs) Handles Time.Tick
        Calculations()
        Drawings()
    End Sub
    Private Sub Mouse_Down(sender As Object, e As MouseEventArgs) Handles MyBase.MouseMove
        balls.Add(GetRandomBall(e.X, e.Y))
    End Sub

    Public numBalls As Double = 20 'lower means more compressed
    Public gravity As Double = 0.3
    Public friction As Double = -0.9
    Public AirResistance As Double = 0.99
    Public CollisionResistance As Double = 0.99
    Public balls As New List(Of Ball)
    Private Sub Init()
        For i = 0 To numBalls - 1 Step 1
            balls.Add(GetRandomBall)
        Next
    End Sub
    Private Function GetRandomBall() As Ball
        Return New Ball(Rnd.Next(0, Me.Width), Rnd.Next(0, Me.Height),
                                Rnd.Next(30, 70), balls.Count, balls,
                                Color.FromArgb(Rnd.Next(0, 255),
                                               Rnd.Next(0, 255),
                                               Rnd.Next(0, 255)), 1)
    End Function
    Private Function GetRandomBall(x As Integer, y As Integer) As Ball
        Return New Ball(x, y,
                                Rnd.Next(30, 70), balls.Count, balls,
                                Color.FromArgb(Rnd.Next(0, 255),
                                               Rnd.Next(0, 255),
                                               Rnd.Next(0, 255)), 1)
    End Function
    Private Sub Calculations()
        For Each Ball As Ball In balls
            Ball.collide()
            Ball.move()
            Ball.ApplyResistiveForces()
        Next
    End Sub
    Private Sub Drawings()
        g.Graphics.Clear(Color.White)
        For Each Ball As Ball In balls
            Ball.Draw(g.Graphics)
        Next
        g.Render()
    End Sub
End Class
Public Class Ball
    Public x, y As Double
    Public vx, vy As Double
    Public diameter As Double
    Public ID As Double
    Public spring As Double
    Public Color As Color
    Public Others As List(Of Ball)
    Sub New(xin As Double, yin As Double, din As Double, idin As Double, othin As List(Of Ball), C As Color, springin As Double)
        Me.x = xin
        Me.y = yin
        Me.diameter = din
        Me.ID = idin
        Me.Others = othin
        Me.Color = C
        Me.spring = springin
    End Sub
    Public Sub collide()
        For i = 0 To Me.Others.Count - 1
            If i <> ID Then
                Dim dx = Others(i).x - x
                Dim dy = Others(i).y - y
                Dim distance = Math.Sqrt(dx * dx + dy * dy)
                Dim minDist = Others(i).diameter / 2 + diameter / 2
                If distance < minDist Then
                    Dim angle = Math.Atan2(dy, dx)
                    Dim targetX = x + Math.Cos(angle) * minDist
                    Dim targetY = y + Math.Sin(angle) * minDist
                    Dim ax = (targetX - Others(i).x) * Me.spring
                    Dim ay = (targetY - Others(i).y) * Me.spring
                    Me.vx -= ax
                    Me.vy -= ay
                    Others(i).vx += ax
                    Others(i).vy += ay

                    Me.vx *= Form1.CollisionResistance
                    Me.vy *= Form1.CollisionResistance
                    Others(i).vx *= Form1.CollisionResistance
                    Others(i).vy *= Form1.CollisionResistance
                End If
            End If
        Next
    End Sub
    Public Sub move()
        Me.vy += Form1.gravity
        Me.x += Me.vx
        Me.y += Me.vy
        If (Me.x + Me.diameter / 2 > Form1.ClientSize.Width) Then
            Me.x = Form1.ClientSize.Width - Me.diameter / 2
            Me.vx *= Form1.friction
        ElseIf (Me.x - Me.diameter / 2 < 0) Then
            Me.x = Me.diameter / 2
            Me.vx *= Form1.friction
        End If
        If (Me.y + Me.diameter / 2 > Form1.ClientSize.Height) Then
            Me.y = Form1.ClientSize.Height - Me.diameter / 2
            Me.vy *= Form1.friction
        ElseIf (Me.y - Me.diameter / 2 < 0) Then
            Me.y = Me.diameter / 2
            Me.vy *= Form1.friction
        End If
    End Sub
    Public Sub ApplyResistiveForces()
        Me.vx *= Form1.AirResistance
        Me.vy *= Form1.AirResistance
    End Sub
    Public Sub Draw(ByRef g As Graphics)
        Dim B As New SolidBrush(Me.Color)
        g.FillEllipse(B, CSng(Me.x - Me.diameter / 2), CSng(Me.y - Me.diameter / 2), CSng(diameter), CSng(diameter))
        B.Dispose()
    End Sub
End Class
