$content = @'
<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" />
    </Found>
    <NotFound>
        <LayoutView>
            <div class="container mt-5">
                <div class="alert alert-warning">
                    <h4>Pagina no encontrada</h4>
                    <a href="/">Volver al inicio</a>
                </div>
            </div>
        </LayoutView>
    </NotFound>
</Router>
'@

[System.IO.File]::WriteAllText("c:\asis\SistemIA.Actualizador\App.razor", $content, [System.Text.Encoding]::UTF8)
Write-Host "App.razor escrito correctamente"
