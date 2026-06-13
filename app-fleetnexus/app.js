require('dotenv').config();
const express = require('express');
const pool = require('./config/db');
const carrierRoutes = require('./routes/carriers');

const app = express();
const port = process.env.PORT || 3000;

// Test database connection on startup
(async () => {
  try {
    const client = await pool.connect();
    console.log('✅ Database connection successful');
    client.release();
  } catch (err) {
    console.error('❌ Database connection failed:', err.message);
    console.error('Ensure DATABASE_URL or DB_* environment variables are configured');
    process.exit(1);  // Fail fast if DB is unavailable
  }
})();

app.use('/', carrierRoutes);

app.listen(port, () => {
  console.log(`FleetNexus API is live at http://localhost:${port}/top-carriers`);
  console.log(`Environment: ${process.env.NODE_ENV || 'development'}`);
});