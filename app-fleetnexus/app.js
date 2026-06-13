require('dotenv').config();
const express = require('express');
const pool = require('./config/db');
const carrierRoutes = require('./routes/carriers');

const app = express();
const port = process.env.PORT || 3000;

(async () => {
  try {
    const client = await pool.connect();
    console.log('Postgres connection test succeeded');
    client.release();
  } catch (err) {
    console.error('Postgres connection test failed:', err);
  }
})();

app.use('/', carrierRoutes);

app.listen(port, () => {
  console.log(`FleetNexus API is live at http://localhost:${port}/top-carriers`);
});