<template>
  <div>
    <router-link to="/turnos" style="font-size:14px; color:#1a73e8">← Volver a turnos</router-link>
    <div v-if="turno" class="card" style="margin-top: 20px; max-width: 560px">
      <h2>Turno #{{ turno.id }}</h2>
      <div class="detail-row"><span class="label">Paciente</span><span>{{ turno.pacienteNombre }}</span></div>
      <div class="detail-row"><span class="label">Médico</span><span>{{ turno.medicoNombre }}</span></div>
      <div class="detail-row"><span class="label">Especialidad</span><span>{{ turno.medicoEspecialidad }}</span></div>
      <div class="detail-row"><span class="label">Fecha y hora</span><span>{{ formatFecha(turno.fechaHora) }}</span></div>
      <div class="detail-row"><span class="label">Estado</span><span>{{ turno.estado }}</span></div>
      <div class="detail-row"><span class="label">Motivo</span><span>{{ turno.motivo }}</span></div>
      <div v-if="turno.fechaCancelacion" class="detail-row">
        <span class="label">Cancelado el</span>
        <span>{{ formatFecha(turno.fechaCancelacion) }}</span>
      </div>

      <template v-if="puedeCambiarEstado">
        <div style="margin-top: 24px">
          <div class="form-group">
            <label>Cambiar estado</label>
            <select v-model="nuevoEstado">
              <option v-for="e in estadosDisponibles" :key="e" :value="e">{{ e }}</option>
            </select>
          </div>
          <button @click="cambiarEstado" style="margin-bottom: 16px">Actualizar estado</button>
        </div>
      </template>

      <div style="display: flex; gap: 10px; margin-top: 16px">
        <button
          class="btn-danger"
          @click="cancelar"
          :disabled="!puedeCancelar"
        >Cancelar turno</button>
        <button
          @click="marcarAusencia"
          :disabled="!puedeMarcarAusencia"
        >Marcar ausencia</button>
      </div>
    </div>
    <p v-else>Cargando...</p>
  </div>
</template>

<script>
import { turnosApi } from '../services/api'

export default {
  name: 'TurnoDetalle',
  data() {
    return {
      turno: null,
      nuevoEstado: ''
    }
  },
  computed: {
    puedeCancelar() {
      return this.turno?.estado === 'Pendiente' || this.turno?.estado === 'Confirmado'
    },
    puedeMarcarAusencia() {
      return this.turno?.estado === 'Pendiente' || this.turno?.estado === 'Confirmado'
    },
    puedeCambiarEstado() {
      return this.turno?.estado === 'Pendiente' || this.turno?.estado === 'Confirmado'
    },
    estadosDisponibles() {
      if (this.turno?.estado === 'Pendiente')  return ['Confirmado', 'Cancelado']
      if (this.turno?.estado === 'Confirmado') return ['Atendido', 'Cancelado']
      return []
    }
  },
  async mounted() {
    try {
      const res = await turnosApi.getById(this.$route.params.id)
      this.turno = res.data
      this.nuevoEstado = this.estadosDisponibles[0] ?? ''
    } catch {
      alert('Error al cargar el turno.')
    }
  },
  methods: {
    formatFecha(fecha) {
      return new Date(fecha).toLocaleString('es-AR')
    },
    async cambiarEstado() {
      try {
        const res = await turnosApi.actualizarEstado(this.turno.id, { estado: this.nuevoEstado })
        this.turno = res.data
        this.nuevoEstado = this.estadosDisponibles[0] ?? ''
      } catch (err) {
        alert(err.response?.data?.mensaje || 'Error al actualizar el estado.')
      }
    },
    async cancelar() {
      if (!confirm('¿Confirmás la cancelación del turno?')) return
      try {
        const res = await turnosApi.cancelar(this.turno.id)
        this.turno = res.data
      } catch (err) {
        alert(err.response?.data?.mensaje || 'Error al cancelar el turno.')
      }
    },
    async marcarAusencia() {
      if (!confirm('¿Confirmás que el paciente no se presentó?')) return
      try {
        const res = await turnosApi.marcarAusencia(this.turno.id)
        this.turno = res.data
      } catch (err) {
        alert(err.response?.data?.mensaje || 'Error al marcar la ausencia.')
      }
    }
  }
}
</script>

<style scoped>
.detail-row {
  display: flex;
  padding: 10px 0;
  border-bottom: 1px solid #f0f0f0;
  font-size: 14px;
}
.label {
  width: 130px;
  font-weight: 600;
  color: #666;
  flex-shrink: 0;
}
</style>