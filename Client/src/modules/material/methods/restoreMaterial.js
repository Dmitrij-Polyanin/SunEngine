export default function () {
  const restoreDialogTitle = this.$tl('restoreDialogTitle');
  const restoreDialogMessage = this.$tl('restoreDialogMessage');
  const okBtn = this.$tl('restoreDialogOk');
  const cancelBtn = this.$tl('restoreDialogCancel');
  this.$q.dialog({
    title: restoreDialogTitle,
    message: restoreDialogMessage,
    ok: okBtn,
    cancel: cancelBtn
  }).onOk(async () => {
    await this.$store.dispatch("request",
      {
        url: "/Materials/Restore",
        data: {
          id: this.material.id,
        }
      }).then(
      () => {
        const restoreSuccessMsg = this.$tl('restoreSuccess');
        this.$successNotify(restoreSuccessMsg);
        this.$router.push(this.category.getRoute());
      }).catch((x) => {
      console.log("error", x)
    });
  });
}
