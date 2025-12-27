window.blazorSubmitForm = function (url, data) {
  const form = document.createElement('form');
  form.method = 'POST';
  form.action = url;
  for (const [key, value] of Object.entries(data || {})) {
    const input = document.createElement('input');
    input.type = 'hidden';
    input.name = key;
    input.value = value;
    form.appendChild(input);
  }
  document.body.appendChild(form);
  form.submit();
  document.body.removeChild(form);
};
