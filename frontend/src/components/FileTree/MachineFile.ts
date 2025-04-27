export type MachineFile = {
  name: string;
  children: MachineFile[] | undefined; //directory case
}