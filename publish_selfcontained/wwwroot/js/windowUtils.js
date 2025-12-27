window.safeOpen = function(url, target) {
  try {
    const w = window.open(url, target || '_blank');
    return !!w;
  } catch (e) {
    console.debug('safeOpen blocked or extension error:', e);
    return false;
  }
};
