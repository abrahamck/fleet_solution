const express = require('express');
const router = express.Router();
const pool = require('../config/db');

// Main endpoint to fetch top 10 carriers
router.get('/top-carriers', async (req, res) => {
  try {
    const query = `
      SELECT dot_number, legal_name, phy_city, phy_state, nbr_power_unit 
      FROM fmcsa_census 
      ORDER BY nbr_power_unit DESC 
      LIMIT 10
    `;
    
    const result = await pool.query(query);
    
    // Return the data as a clean JSON object
    res.json({
      success: true,
      count: result.rows.length,
      data: result.rows
    });
  } catch (err) {
    console.error(err);
    res.status(500).json({ success: false, error: "Database query failed" });
  }
});

module.exports = router;