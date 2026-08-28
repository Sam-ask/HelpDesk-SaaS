import { Component, OnInit, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',  
  styleUrl: './app.css'       
})
export class App implements OnInit {
  private http = inject(HttpClient);
  
  tickets: any[] = [];
  ticketsFiltrados: any[] = []; 
  listaUsuarios: any[] = [];
  listaTecnicos: any[] = [];

  email = '';
  password = '';
  mensajeError = '';
  
  // VARIABLES DEL FORMULARIO
  nuevoTitulo = '';
  nuevaDescripcion = '';
  nuevoEstadoId: number = 1;
  nuevaCategoriaId: number = 1; 
  nuevaPrioridadId: number = 1; 
  nuevoComentario: string = ''; 

  ticketEditando: number | null = null; 
  mostrarFormulario: boolean = false; 
  
  vistaActual: string = 'dashboard';

  cambiarVista(nuevaVista: string) {
    this.vistaActual = nuevaVista;
    this.mostrarFormulario = false; 
  }
  
  totalTickets: number = 0;
  ticketsAbiertos: number = 0;
  ticketsEnProceso: number = 0;
  ticketsResueltos: number = 0;
  tasaResolucion: number = 0; 
  ticketsSinAsignar: number = 0; 
  ticketsUrgentes: number = 0; 
  metricasCategorias: any[] = [];
  metricasPrioridades: any[] = [];

  token = localStorage.getItem('token') || ''; 
  usuarioActualId: number = 0; 
  usuarioActualRol: string | number = 0; 
  usuarioActualNombre: string = 'Usuario'; 

  historialSeleccionado: any = null; 
  ticketHistorialId: number = 0;

  filtroTexto: string = '';        
  filtroSolicitante: string = '';  
  filtroTecnico: string = '';      
  filtroEstatus: number = 0;       

  ngOnInit() {
    if (this.token) {
      this.procesarToken(this.token);
      this.obtenerTickets();
      this.obtenerUsuarios();
    }
  }

  hacerLogin() {
    const credenciales = { email: this.email, password: this.password };
    this.mensajeError = '';

    this.http.post<any>('https://localhost:7037/api/auth/login', credenciales)
      .subscribe({
        next: (respuesta) => {
          this.token = respuesta.token;
          localStorage.setItem('token', this.token); 
          this.procesarToken(this.token);
          this.obtenerTickets(); 
          this.obtenerUsuarios();
        },
        error: (err) => {
          this.mensajeError = 'Correo o contraseña incorrectos.';
        }
      });
  }

  procesarToken(token: string) {
    try {
      const payloadDecodificado = JSON.parse(atob(token.split('.')[1])); 
      this.usuarioActualId = parseInt(payloadDecodificado.sub, 10) || 0;
      const rol = payloadDecodificado['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] 
                  || payloadDecodificado.role || payloadDecodificado.RolId;
      this.usuarioActualRol = rol ? (parseInt(rol, 10) || rol) : 0;
      this.usuarioActualNombre = payloadDecodificado.name || payloadDecodificado.unique_name || payloadDecodificado.Nombre || 'Usuario';
    } catch (e) {
      this.usuarioActualId = 0;
      this.usuarioActualRol = 0;
      this.usuarioActualNombre = 'Usuario';
    }
  }

  obtenerUsuarios() {
    const headers = new HttpHeaders({ 'Authorization': `Bearer ${this.token}` });
    this.http.get('https://localhost:7037/api/usuarios', { headers })
      .subscribe({
        next: (datos: any) => {
          this.listaUsuarios = datos;
          this.listaTecnicos = datos.filter((u: any) => u.rolNombre === 'Técnico' || u.rolNombre === 'Tecnico' || u.rolNombre === 'Administrador' || u.rolNombre === 'Admin');
        },
        error: (err) => console.error("Error al cargar usuarios", err)
      });
  }

  obtenerTickets() {
    const headers = new HttpHeaders({ 'Authorization': `Bearer ${this.token}` });
    this.http.get('https://localhost:7037/api/tickets', { headers })
      .subscribe({
        next: (datos: any) => { 
          if (this.usuarioActualRol === 'Administrador' || this.usuarioActualRol === 'Admin' || this.usuarioActualRol === 'Tecnico' || this.usuarioActualRol === 'Técnico') {
            this.tickets = datos; 
          } else {
            this.tickets = datos.filter((ticket: any) => ticket.creadorId === this.usuarioActualId);
          }
          this.ticketsFiltrados = [...this.tickets]; 
          this.calcularEstadisticas(); 
        },
        error: (err) => { if (err.status === 401) this.cerrarSesion(); }
      });
  }

  calcularEstadisticas() {
    // ESTADÍSTICAS BÁSICAS
    this.totalTickets = this.tickets.length;
    this.ticketsAbiertos = this.tickets.filter(t => t.estadoId === 1).length;
    this.ticketsEnProceso = this.tickets.filter(t => t.estadoId === 2 || t.estadoId === 3).length;
    this.ticketsResueltos = this.tickets.filter(t => t.estadoId === 4 || t.estadoId === 5).length;
    this.tasaResolucion = this.totalTickets > 0 ? Math.round((this.ticketsResueltos / this.totalTickets) * 100) : 0;
    this.ticketsSinAsignar = this.tickets.filter(t => !t.tecnico).length;
    this.ticketsUrgentes = this.tickets.filter(t => t.prioridadId === 4 && t.estadoId !== 4 && t.estadoId !== 5).length;

    const total = this.totalTickets > 0 ? this.totalTickets : 1; 

    this.metricasCategorias = [
      { nombre: 'Hardware', count: this.tickets.filter(t => t.categoriaId === 1).length, color: 'bg-primary' },
      { nombre: 'Software', count: this.tickets.filter(t => t.categoriaId === 2).length, color: 'bg-info' },
      { nombre: 'Redes / Internet', count: this.tickets.filter(t => t.categoriaId === 3).length, color: 'bg-warning' },
      { nombre: 'Accesos / Cuentas', count: this.tickets.filter(t => t.categoriaId === 4).length, color: 'bg-success' },
      { nombre: 'Otro', count: this.tickets.filter(t => t.categoriaId === 5).length, color: 'bg-secondary' }
    ].map(c => ({ ...c, porcentaje: Math.round((c.count / total) * 100) })).sort((a,b) => b.count - a.count); // Los ordenamos de mayor a menor

    this.metricasPrioridades = [
      { nombre: 'Urgente', count: this.tickets.filter(t => t.prioridadId === 4).length, icono: 'bi-exclamation-circle-fill', txt: 'text-danger', bg: '#FEE2E2' },
      { nombre: 'Alta', count: this.tickets.filter(t => t.prioridadId === 3).length, icono: 'bi-arrow-up-circle-fill', txt: 'text-warning', bg: '#FEFCE8' },
      { nombre: 'Media', count: this.tickets.filter(t => t.prioridadId === 2).length, icono: 'bi-dash-circle-fill', txt: 'text-primary', bg: '#EFF6FF' },
      { nombre: 'Baja', count: this.tickets.filter(t => t.prioridadId === 1).length, icono: 'bi-arrow-down-circle-fill', txt: 'text-secondary', bg: '#F1F5F9' }
    ];
  }
   

  filtrarTickets() {
    let resultado = [...this.tickets];
    if (this.filtroTexto.trim()) {
      const term = this.filtroTexto.toLowerCase().trim();
      resultado = resultado.filter(t => t.id.toString().includes(term) || t.titulo.toLowerCase().includes(term));
    }
    if (this.filtroSolicitante.trim()) {
      const term = this.filtroSolicitante.toLowerCase().trim();
      resultado = resultado.filter(t => (t.creador?.nombre || '').toLowerCase().includes(term));
    }
    if (this.filtroTecnico.trim()) {
      const term = this.filtroTecnico.toLowerCase().trim();
      resultado = resultado.filter(t => (t.tecnico?.nombre || '').toLowerCase().includes(term));
    }
    if (this.filtroEstatus !== 0) {
      resultado = resultado.filter(t => t.estadoId === this.filtroEstatus);
    }
    this.ticketsFiltrados = resultado;
  }

  limpiarFiltros() {
    this.filtroTexto = '';
    this.filtroSolicitante = '';
    this.filtroTecnico = '';
    this.filtroEstatus = 0;
    this.filtrarTickets(); 
  }

  exportarExcel() {
    const cabeceras = ['Folio', 'Prioridad', 'Categoría', 'Asunto', 'Solicitante', 'Asignado a', 'Estatus'];
    const filas = this.ticketsFiltrados.map(t => [
      t.id, 
      this.obtenerPrioridad(t.prioridadId).nombre,
      this.obtenerNombreCategoria(t.categoriaId),
      `"${t.titulo}"`, 
      t.creador?.nombre || 'Desconocido', 
      t.tecnico?.nombre || 'Sin asignar', 
      this.obtenerNombreEstado(t.estadoId)
    ]);
    const contenidoCsv = [cabeceras.join(',')].concat(filas.map(fila => fila.join(','))).join('\n');
    const blob = new Blob(['\uFEFF' + contenidoCsv], { type: 'text/csv;charset=utf-8;' });
    const url = window.URL.createObjectURL(blob);
    const enlace = document.createElement('a');
    enlace.href = url;
    enlace.download = `Reporte_Tickets_${new Date().toISOString().split('T')[0]}.csv`;
    enlace.click();
    window.URL.revokeObjectURL(url);
  }

  cerrarSesion() {
    this.token = '';
    this.tickets = [];
    this.ticketsFiltrados = [];
    this.mostrarFormulario = false;
    this.vistaActual = 'dashboard';
    this.cerrarHistorial();
    localStorage.removeItem('token');
  }

  abrirFormularioCrear() {
    this.cancelarEdicion(); 
    this.mostrarFormulario = true; 
  }

  cargarParaEditar(ticket: any) {
    this.nuevoTitulo = ticket.titulo;
    this.nuevaDescripcion = ticket.descripcion;
    this.nuevoEstadoId = ticket.estadoId || 1; 
    this.nuevaCategoriaId = ticket.categoriaId || 1; 
    this.nuevaPrioridadId = ticket.prioridadId || 1;
    this.ticketEditando = ticket.id; 
    this.mostrarFormulario = true; 
  }

  cancelarEdicion() {
    this.nuevoTitulo = '';
    this.nuevaDescripcion = '';
    this.nuevoEstadoId = 1; 
    this.nuevaCategoriaId = 1; 
    this.nuevaPrioridadId = 1;
    this.nuevoComentario = ''; 
    this.ticketEditando = null;
    this.mostrarFormulario = false; 
  }

  guardarTicket() {
    if (!this.nuevoTitulo || !this.nuevaDescripcion) return; 

    const headers = new HttpHeaders({ 'Authorization': `Bearer ${this.token}` });

    if (this.ticketEditando) {
      const paqueteUpdate = {
        id: this.ticketEditando, titulo: this.nuevoTitulo, descripcion: this.nuevaDescripcion,
        creadorId: this.usuarioActualId, estadoId: this.nuevoEstadoId, 
        categoriaId: this.nuevaCategoriaId, prioridadId: this.nuevaPrioridadId,
        comentario: this.nuevoComentario 
      };
      this.http.put(`https://localhost:7037/api/tickets/${this.ticketEditando}`, paqueteUpdate, { headers, responseType: 'text' })
        .subscribe({ next: () => { this.cancelarEdicion(); this.obtenerTickets(); }});
    } else {
      const paqueteNuevo = {
        titulo: this.nuevoTitulo, descripcion: this.nuevaDescripcion, creadorId: this.usuarioActualId, 
        estadoId: this.nuevoEstadoId, categoriaId: this.nuevaCategoriaId, prioridadId: this.nuevaPrioridadId 
      };
      this.http.post('https://localhost:7037/api/tickets', paqueteNuevo, { headers })
        .subscribe({ next: () => { this.cancelarEdicion(); this.obtenerTickets(); }});
    }
  }

  eliminarTicket(id: number) {
    if (!confirm(`¿Estás seguro de que deseas eliminar el ticket #${id}?`)) return;
    const headers = new HttpHeaders({ 'Authorization': `Bearer ${this.token}` });
    this.http.delete(`https://localhost:7037/api/tickets/${id}`, { headers, responseType: 'text' })
      .subscribe({ next: () => this.obtenerTickets() });
  }

  asignarmeTicket(ticket: any) {
    if (!confirm(`¿Deseas asignarte el ticket #${ticket.id} para resolverlo?`)) return;
    const headers = new HttpHeaders({ 'Authorization': `Bearer ${this.token}` });
    const paqueteAsignacion = { tecnicoId: this.usuarioActualId, nuevoEstadoId: 2, comentario: "El técnico ha tomado el ticket." };
    this.http.put(`https://localhost:7037/api/tickets/${ticket.id}/asignar`, paqueteAsignacion, { headers })
      .subscribe({ next: () => { this.obtenerTickets(); }});
  }

  verHistorial(id: number) {
    const headers = new HttpHeaders({ 'Authorization': `Bearer ${this.token}` });
    this.http.get(`https://localhost:7037/api/tickets/${id}/historial`, { headers })
      .subscribe({
        next: (datos: any) => { this.historialSeleccionado = datos; this.ticketHistorialId = id; }
      });
  }

  cerrarHistorial() { this.historialSeleccionado = null; this.ticketHistorialId = 0; }

  // --- DICCIONARIOS DE TRADUCCIÓN ---
  obtenerNombreEstado(id: number): string {
    const estados: any = { 1: 'Abierto', 2: 'Asignado', 3: 'En Proceso', 4: 'Resuelto', 5: 'Cerrado' };
    return estados[id] || 'Desconocido';
  }

  obtenerNombreCategoria(id: number): string {
    const categorias: any = { 1: 'Hardware', 2: 'Software', 3: 'Redes / Internet', 4: 'Accesos / Cuentas', 5: 'Otro' };
    return categorias[id] || 'General';
  }

  obtenerPrioridad(id: number): any {
    const prioridades: any = { 
      1: { nombre: 'Baja', colorClase: 'text-secondary', icono: 'bi-arrow-down-circle-fill' }, 
      2: { nombre: 'Media', colorClase: 'text-primary', icono: 'bi-dash-circle-fill' }, 
      3: { nombre: 'Alta', colorClase: 'text-warning', icono: 'bi-arrow-up-circle-fill' }, 
      4: { nombre: 'Urgente', colorClase: 'text-danger', icono: 'bi-exclamation-circle-fill' } 
    };
    return prioridades[id] || prioridades[1];
  }

  formatearFolio(id: number): string {
    return '#' + id.toString().padStart(3, '0');
  }

  obtenerInicial(nombre: string): string {
    return nombre ? nombre.charAt(0).toUpperCase() : 'U';
  }
}