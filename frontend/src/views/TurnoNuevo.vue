<template>
  <div class="card" style="max-width: 560px">
    <h2>Nuevo turno</h2>
    <div class="form-group">
      <label>Paciente</label>
      <select v-model="form.pacienteId">
        <option value="">Seleccioná un paciente</option>
        <option v-for="p in pacientes" :key="p.id" :value="p.id">
          {{ p.nombreCompleto }} — DNI {{ p.dni }}
        </option>
      </select>
      <span v-if="errors.pacienteId" class="error">{{ errors.pacienteId }}</span>
    </div>
    <div class="form-group">
      <label>Médico</label>
      <select v-model="form.medicoId">
        <option value="">Seleccioná un médico</option>
        <option v-for="m in medicos" :key="m.id" :value="m.id">
          {{ m.nombreCompleto }} — {{ m.especialidad }}
        </option>
      </select>
      <span v-if="errors.medicoId" class="error">{{ errors.medicoId }}</span>
    </div>
    <div class="form-group">
      <label>Fecha y hora</label>
      <input type="datetime-local" v-model="form.fechaHora" :min="fechaMinima" />
      <span v-if="errors.fechaHora" class="error">{{ errors.fechaHora }}</span>
    </div>
    <div class="form-group">
      <label>Motivo</label>
      <input type="text" v-model="form.motivo" placeholder="Motivo de la consulta" />
      <span v-if="errors.motivo" class="error">{{ errors.motivo }}</span>
    </div>
    <button @click="guardar">Confirmar turno</button>
  </div>
</template>

<script>
import { turnosApi, pacientesApi, medicosApi } from '../services/api'

export default {
  name: 'TurnoNuevo',
  data() {
    return {
      form: {
        pacienteId: '',
        medicoId: '',
        fechaHora: '',
        motivo: ''
      },
      errors: {},
      pacientes: [],
      medicos: []
    }
  },
  computed: {
    fechaMinima() {
      const now = new Date()
      now.setMinutes(now.getMinutes() - now.getTimezoneOffset())
      return now.toISOString().slice(0, 16)
    }
  },
  async mounted() {
    try {
      const [pRes, mRes] = await Promise.all([pacientesApi.getAll(), medicosApi.getAll()])
      this.pacientes = pRes.data
      this.medicos = mRes.data
    } catch {
      alert('Error al cargar los datos.')
    }
  },
  methods: {
    validar() {
      const errors = {}
      if (!this.form.pacienteId) errors.pacienteId = 'Seleccioná un paciente.'
      if (!this.form.medicoId)   errors.medicoId   = 'Seleccioná un médico.'
      if (!this.form.fechaHora)  errors.fechaHora  = 'Ingresá una fecha y hora.'
      if (!this.form.motivo.trim()) errors.motivo  = 'Ingresá el motivo de la consulta.'
      this.errors = errors
      return Object.keys(errors).length === 0
    },
    async guardar() {
      if (!this.validar()) return
      try {
        await turnosApi.create({
          pacienteId: Number(this.form.pacienteId),
          medicoId:   Number(this.form.medicoId),
          fechaHora:  this.form.fechaHora,
          motivo:     this.form.motivo.trim()
        })
        this.$router.push('/turnos')
      } catch (err) {
        alert(err.response?.data?.mensaje || 'Error al crear el turno.')
      }
    }
  }
}
</script>

<style scoped>
.error {
  display: block;
  color: #d32f2f;
  font-size: 12px;
  margin-top: 4px;
}
</style>